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

    public RabbitMQMessageBus(ILogger<RabbitMQMessageBus> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Tentar conectar de forma assíncrona para não bloquear a inicialização
        _ = Task.Run(() => TryConnectAsync());
    }

    private async Task TryConnectAsync()
    {
        try
        {
            await Task.Delay(2000); // Aguardar um pouco para os serviços ficarem prontos
            
            var rabbitMqConfig = _configuration.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig["HostName"] ?? "localhost",
                UserName = rabbitMqConfig["UserName"] ?? "guest",
                Password = rabbitMqConfig["Password"] ?? "guest",
                VirtualHost = rabbitMqConfig["VirtualHost"] ?? "/",
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                SocketReadTimeout = TimeSpan.FromSeconds(5)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ. A aplicação continuará funcionando sem mensageria.");
        }
    }

    public async Task PublishAsync<T>(T message, string queue, CancellationToken cancellationToken = default) where T : class
    {
        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ não está disponível. Mensagem não será publicada na fila {Queue}", queue);
            return;
        }

        try
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: null,
                body: body);

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
        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            _logger.LogWarning("RabbitMQ não está disponível. Consumidor não será configurado para a fila {Queue}", queue);
            return;
        }

        try
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

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
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Mensagem processada com sucesso da fila {Queue}", queue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila {Queue}", queue);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: queue,
                                 autoAck: false,
                                 consumer: consumer);

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
        
        _channel?.Dispose();
        _connection?.Dispose();
        _disposed = true;
    }
}
