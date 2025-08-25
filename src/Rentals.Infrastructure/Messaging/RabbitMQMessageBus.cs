using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rentals.Application.Abstractions;
using System.Text;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Rentals.Infrastructure.Messaging;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private IConnection? _connection;
    private RabbitMQ.Client.IModel? _channel;
    private readonly ILogger<RabbitMQMessageBus> _logger;
    private readonly IConfiguration _configuration;
    private readonly object _lock = new object();
    private bool _disposed = false;
    private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
    private Task? _connectionTask;

    public RabbitMQMessageBus(ILogger<RabbitMQMessageBus> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Iniciar tentativa de conexão em background
        _connectionTask = Task.Run(() => TryConnectWithRetryAsync());
    }

    private async Task TryConnectWithRetryAsync()
    {
        const int maxRetries = 10;
        const int delaySeconds = 3;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Tentativa {Attempt}/{MaxRetries} de conexão com RabbitMQ...", attempt, maxRetries);
                
                var rabbitMqConfig = _configuration.GetSection("RabbitMQ");
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMqConfig["HostName"] ?? "localhost",
                    UserName = rabbitMqConfig["UserName"] ?? "guest",
                    Password = rabbitMqConfig["Password"] ?? "guest",
                    VirtualHost = rabbitMqConfig["VirtualHost"] ?? "/",
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(10),
                    SocketReadTimeout = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso na tentativa {Attempt}", attempt);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tentativa {Attempt}/{MaxRetries} falhou. Aguardando {Delay} segundos antes da próxima tentativa...", attempt, maxRetries, delaySeconds);
                
                if (attempt < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    _logger.LogError(ex, "Todas as {MaxRetries} tentativas de conexão com RabbitMQ falharam. A aplicação continuará funcionando sem mensageria.", maxRetries);
                }
            }
        }
    }

    public async Task WaitForConnectionAsync(TimeSpan timeout = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromMinutes(2);

        if (_connectionTask != null)
        {
            try
            {
                await _connectionTask.WaitAsync(timeout);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout aguardando conexão com RabbitMQ. Continuando sem mensageria.");
            }
        }
    }

    public async Task PublishAsync<T>(T message, string queue, CancellationToken cancellationToken = default) where T : class
    {
        await WaitForConnectionAsync();
        
        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ não está disponível. Mensagem não será publicada na fila {Queue}", queue);
            return;
        }

        try
        {
            lock (_lock)
            {
                _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            }

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            lock (_lock)
            {
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queue,
                    basicProperties: null,
                    body: body);
            }

            _logger.LogInformation("Mensagem publicada na fila {Queue}: {Message}", queue, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem na fila {Queue}", queue);
            // Não lançar exceção para não quebrar a aplicação
        }
    }

    public async Task SubscribeAsync<T>(string queue, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        await WaitForConnectionAsync();
        
        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ não está disponível. Consumidor não será configurado para a fila {Queue}", queue);
            return;
        }

        try
        {
            lock (_lock)
            {
                _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonConvert.DeserializeObject<T>(message);

                    if (deserializedMessage != null)
                    {
                        await handler(deserializedMessage);
                        
                        lock (_lock)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        
                        _logger.LogInformation("Mensagem processada com sucesso da fila {Queue}", queue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila {Queue}", queue);
                    
                    lock (_lock)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
            };

            lock (_lock)
            {
                _channel.BasicConsume(queue: queue,
                                     autoAck: false,
                                     consumer: consumer);
            }

            _logger.LogInformation("Consumidor iniciado para a fila {Queue}", queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar consumidor para a fila {Queue}", queue);
            // Não lançar exceção para não quebrar a aplicação
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _connectionSemaphore?.Dispose();
        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;
    }
}
