# 🏍️ Rentals API

Sistema de gerenciamento de locação de motocicletas desenvolvido com **Clean Architecture** e **Domain-Driven Design (DDD)**, seguindo os princípios **SOLID**.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Uso](#uso)
- [API Endpoints](#api-endpoints)
- [Serviços Externos](#serviços-externos)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Contribuição](#contribuição)

---

## 🎯 Visão Geral

A **Rentals API** é um sistema completo para gerenciamento de locação de motocicletas, oferecendo funcionalidades para:

- **Gestão de Entregadores**: Cadastro e gerenciamento de motoristas
- **Gestão de Motocicletas**: Controle do parque de motos disponíveis
- **Planos de Locação**: Criação e gerenciamento de planos de aluguel
- **Sistema de Locação**: Processo completo de aluguel e devolução
- **Notificações**: Sistema de eventos para motocicletas de 2024
- **Autenticação**: Sistema JWT para controle de acesso

### 🏗️ Características da Arquitetura

- ✅ **Clean Architecture** com separação clara de responsabilidades
- ✅ **Domain-Driven Design (DDD)** com entidades ricas
- ✅ **Princípios SOLID** aplicados rigorosamente
- ✅ **Injeção de Dependência** para baixo acoplamento
- ✅ **Event-Driven Architecture** com RabbitMQ
- ✅ **Multi-database** (PostgreSQL + MongoDB)
- ✅ **Object Storage** com MinIO

---

## 🏛️ Arquitetura

O projeto segue a **Clean Architecture** com as seguintes camadas:

```
┌─────────────────────────────────────┐
│           Presentation              │ ← Controllers, DTOs
├─────────────────────────────────────┤
│           Application               │ ← Use Cases, Handlers
├─────────────────────────────────────┤
│             Domain                  │ ← Entities, Value Objects
├─────────────────────────────────────┤
│         Infrastructure             │ ← Repositories, External Services
└─────────────────────────────────────┘
```

### 📁 Estrutura das Camadas

#### **Domain Layer** (`src/Rentals.Domain/`)
- **Entidades**: `Motorcycle`, `DeliveryDriver`, `Rental`, `RentalPlan`
- **Value Objects**: `LicensePlate`, `CNH`, `CNPJ`
- **Domain Events**: `MotorcycleCreatedEvent`
- **Regras de Negócio**: Encapsuladas nas entidades

#### **Application Layer** (`src/Rentals.Application/`)
- **Commands**: Handlers para operações de escrita
- **Queries**: Handlers para operações de leitura
- **Interfaces**: Contratos para repositórios e serviços
- **Services**: Lógica de aplicação

#### **Infrastructure Layer** (`src/Rentals.Infrastructure/`)
- **Repositories**: Implementações de persistência
- **External Services**: RabbitMQ, MinIO, MongoDB
- **Configurations**: Entity Framework, JWT
- **Migrations**: Controle de versão do banco

#### **Presentation Layer** (`src/Rentals.WebApi/`)
- **Controllers**: Endpoints da API
- **Middleware**: Autenticação, logging
- **Configuration**: DI Container, Swagger

---

## 🛠️ Tecnologias

### **Backend**
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM para PostgreSQL
- **JWT Bearer** - Autenticação
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação da API

### **Banco de Dados**
- **PostgreSQL** - Banco relacional principal
- **MongoDB** - Banco NoSQL para notificações

### **Serviços Externos**
- **RabbitMQ** - Message broker para eventos
- **MinIO** - Object storage para imagens
- **Docker** - Containerização dos serviços

### **Ferramentas**
- **Docker Compose** - Orquestração de containers
- **Entity Framework Migrations** - Versionamento do banco

---

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- **.NET 9.0 SDK** ou superior
- **Docker Desktop** com Docker Compose
- **Git** para clonar o repositório
- **Visual Studio 2022** ou **VS Code** (recomendado)

### 🔧 Verificação das Instalações

```bash
# Verificar .NET
dotnet --version

# Verificar Docker
docker --version
docker-compose --version

# Verificar Git
git --version
```

---

## 🚀 Instalação

### **1. Clonar o Repositório**

```bash
git clone <repository-url>
cd RentalsAPI
```

### **2. Iniciar Serviços com Docker**

```bash
# Navegar para o diretório src
cd src

# Iniciar todos os serviços
docker-compose up -d
```

### **3. Verificar Status dos Serviços**

```bash
# Verificar containers rodando
docker ps

# Verificar logs dos serviços
docker-compose logs
```

### **4. Aplicar Migrações do Banco**

```bash
# Voltar para o diretório raiz
cd ..

# Aplicar migrações
dotnet ef database update --project src/Rentals.Infrastructure --startup-project src/Rentals.WebApi
```

### **5. Executar a Aplicação**

```bash
# Executar a API
dotnet run --project src/Rentals.WebApi --urls "http://localhost:5000"
```

---

## ⚙️ Configuração

### **Arquivos de Configuração**

- `appsettings.json` - Configuração base (Estou enviando para evitar a necessidade de configurar.)
- `appsettings.Development.json` - Configuração de desenvolvimento

### **Variáveis de Ambiente**

Crie um arquivo `appsettings.json` baseado no `appsettings.example.json`:

```json
{
  "ConnectionStrings": {
    "RentalsDb": "Host=localhost;Database=rentals;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "rentals-api",
    "Audience": "rentals-clients"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "rentals",
    "MotorcyclesCollectionName": "motorcycles",
    "NotificationsCollectionName": "motorcycle_notifications"
  },
  "Minio": {
    "Endpoint": "http://localhost:9000",
    "AccessKey": "minio",
    "SecretKey": "minio123"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### **Portas dos Serviços**

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| **API** | 5000 | Endpoints da aplicação |
| **PostgreSQL** | 5432 | Banco de dados principal |
| **MongoDB** | 27017 | Banco NoSQL |
| **RabbitMQ** | 5672 | Message broker |
| **RabbitMQ Management** | 15672 | Interface web do RabbitMQ |
| **MinIO API** | 9000 | Object storage |
| **MinIO Console** | 9001 | Interface web do MinIO |

---

## 📖 Uso

### **1. Acessar a API**

Após iniciar a aplicação, acesse:

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### **2. Autenticação**

A API utiliza **JWT Bearer Token** para autenticação:

```bash
# Exemplo de login
curl -X POST "http://localhost:5000/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

### **3. Usar o Token**

```bash
# Exemplo de requisição autenticada
curl -X GET "http://localhost:5000/motos" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔌 API Endpoints

### **Autenticação**
- `POST /auth/login` - Login de usuário
- `POST /auth/registro/entregador` - Registro de entregador
- `POST /auth/registro/admin` - Registro de administrador

### **Entregadores**
- `GET /entregadores` - Listar todos os entregadores
- `POST /entregadores/{id}/cnh` - Upload de imagem da CNH

### **Motocicletas**
- `GET /motos` - Listar todas as motocicletas
- `GET /motos/{id}` - Obter motocicleta por ID
- `POST /motos` - Cadastrar nova motocicleta
- `PUT /motos/{id}/placa` - Atualizar placa
- `DELETE /motos/{id}` - Remover motocicleta
- `GET /motos/2024` - Listar motocicletas de 2024

### **Planos de Locação**
- `GET /planos` - Listar todos os planos
- `POST /planos` - Criar novo plano

### **Locações**
- `GET /locacoes/{id}` - Obter locação por ID
- `POST /locacoes` - Criar nova locação
- `PUT /locacoes/{id}/devolucao` - Devolver motocicleta

---

## 🔧 Serviços Externos

### **🐰 RabbitMQ - Message Broker**

O RabbitMQ é utilizado para comunicação assíncrona entre componentes:

#### **Configuração**
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

#### **Funcionalidades**
- **Eventos de Domínio**: Publicação de eventos quando motocicletas são criadas
- **Notificações**: Sistema de notificação para motocicletas de 2024
- **Processamento Assíncrono**: Operações que não precisam de resposta imediata

#### **Acesso à Interface Web**
- **URL**: http://localhost:15672
- **Usuário**: guest
- **Senha**: guest

#### **Exemplo de Uso**
```csharp
// Publicar evento
await _messageBus.PublishAsync(new MotorcycleCreatedMessage
{
    Id = motorcycle.Id,
    Identifier = motorcycle.Identifier,
    Year = motorcycle.Year
}, "motorcycle.created");

// Consumir evento
await _messageBus.SubscribeAsync<MotorcycleCreatedMessage>(
    "motorcycle.created",
    async message => await ProcessMotorcycleCreated(message));
```

### **📦 MinIO - Object Storage**

O MinIO é utilizado para armazenamento de arquivos (imagens da CNH):

#### **Configuração**
```json
{
  "Minio": {
    "Endpoint": "http://localhost:9000",
    "AccessKey": "minio",
    "SecretKey": "minio123"
  }
}
```

#### **Funcionalidades**
- **Upload de Imagens**: Armazenamento de fotos da CNH
- **Gerenciamento de Buckets**: Organização de arquivos
- **URLs Temporárias**: Geração de links para download

#### **Acesso à Interface Web**
- **URL**: http://localhost:9001
- **Usuário**: minio
- **Senha**: minio123

#### **Exemplo de Uso**
```csharp
// Upload de arquivo
var uploadResult = await _storageService.UploadFileAsync(
    bucketName: "cnh-images",
    fileName: "cnh_123.png",
    fileStream: imageStream,
    contentType: "image/png");

// Download de arquivo
var fileStream = await _storageService.DownloadFileAsync(
    bucketName: "cnh-images",
    fileName: "cnh_123.png");
```

### **🍃 MongoDB - NoSQL Database**

O MongoDB é utilizado para dados não relacionais (notificações):

#### **Configuração**
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "rentals",
    "MotorcyclesCollectionName": "motorcycles",
    "NotificationsCollectionName": "motorcycle_notifications"
  }
}
```

#### **Funcionalidades**
- **Notificações**: Armazenamento de notificações de motocicletas
- **Logs de Eventos**: Histórico de eventos do sistema
- **Dados Flexíveis**: Informações que não se encaixam no modelo relacional

#### **Exemplo de Uso**
```csharp
// Inserir notificação
await _notificationRepository.AddAsync(new MotorcycleNotification
{
    MotorcycleId = motorcycleId,
    Message = "Nova motocicleta 2024 cadastrada",
    CreatedAt = DateTime.UtcNow
});

// Buscar notificações
var notifications = await _notificationRepository.GetByMotorcycleIdAsync(motorcycleId);
```

---

## 📁 Estrutura do Projeto

```
RentalsAPI/
├── src/
│   ├── Rentals.Domain/                 # Camada de Domínio
│   │   ├── Abstractions/              # Interfaces e classes base
│   │   ├── Drivers/                   # Entidades de motoristas
│   │   ├── Users/                     # Entidades de usuários
│   │   └── Vehicles/                  # Entidades de veículos
│   │
│   ├── Rentals.Application/           # Camada de Aplicação
│   │   ├── Abstractions/              # Interfaces de repositórios
│   │   ├── Commands/                  # Handlers de comandos
│   │   ├── Queries/                   # Handlers de consultas
│   │   └── Services/                  # Serviços de aplicação
│   │
│   ├── Rentals.Infrastructure/        # Camada de Infraestrutura
│   │   ├── Auth/                      # Autenticação JWT
│   │   ├── Configurations/            # Configurações EF Core
│   │   ├── Entities/                  # Entidades de infraestrutura
│   │   ├── Images/                    # Processamento de imagens
│   │   ├── Messaging/                 # Integração RabbitMQ
│   │   ├── Migrations/                # Migrações do banco
│   │   ├── MongoDB/                   # Integração MongoDB
│   │   ├── Repositories/              # Implementações de repositórios
│   │   └── Storage/                   # Integração MinIO
│   │
│   ├── Rentals.WebApi/               # Camada de Apresentação
│   │   ├── Auth/                      # Controllers de autenticação
│   │   ├── Drivers/                   # Controllers de motoristas
│   │   ├── Rentals/                   # Controllers de locações
│   │   └── Vehicles/                  # Controllers de veículos
│   │
│   └── docker-compose.yml            # Configuração dos serviços
│
├── README.md                         # Este arquivo
├── Rentals.sln                       # Solution file
└── RELATORIO_SOLID_CLEAN_ARCHITECTURE.md  # Relatório de análise
```

---

## 🧪 Desenvolvimento

### **Comandos Úteis**

```bash
# Build do projeto
dotnet build

# Executar testes (se houver)
dotnet test

# Aplicar migrações
dotnet ef database update --project src/Rentals.Infrastructure --startup-project src/Rentals.WebApi

# Criar nova migração
dotnet ef migrations add NomeDaMigracao --project src/Rentals.Infrastructure --startup-project src/Rentals.WebApi

# Gerar script SQL
dotnet ef migrations script --project src/Rentals.Infrastructure --startup-project src/Rentals.WebApi

# Limpar build
dotnet clean

# Restaurar pacotes
dotnet restore
```

### **Debug e Logs**

```bash
# Ver logs da aplicação
docker-compose logs -f rentals-api

# Ver logs do PostgreSQL
docker-compose logs -f postgres

# Ver logs do RabbitMQ
docker-compose logs -f rabbitmq

# Ver logs do MinIO
docker-compose logs -f minio

# Ver logs do MongoDB
docker-compose logs -f mongo
```

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---
