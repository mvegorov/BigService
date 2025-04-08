#pragma warning disable IDE0005
using Application.Abstractions.Persistence;
using Application.Services;
using Infrastructure.Configurations;
using Infrastructure.Configurations.Http;
using Infrastructure.Migrations;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Connection;
using Infrastructure.Swagger;
using Kafka.Abstractions.Consumer;
using Orders.Kafka.Contracts;
using Presentation.Controllers.Http;
using Presentation.Kafka;
using Presentation.Kafka.Consumer;
using Presentation.Kafka.Producer;

WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.Configure<KafkaProducerOptions>(builder.Configuration.GetSection("KafkaProducer"));
builder.Services.Configure<KafkaConsumerOptions>(builder.Configuration.GetSection("KafkaConsumer"));

builder.Services.AddKafkaProducer<OrderCreationKey, OrderCreationValue>();

builder.Services.AddScoped<IKafkaMessageHandler<OrderProcessingKey, OrderProcessingValue>, OrderProcessingMessageHandler>();
builder.Services.AddKafkaConsumer<OrderProcessingKey, OrderProcessingValue>();

builder.Services
    .Configure<ConfigurationUpdateSettings>(builder.Configuration.GetSection("ConfigurationUpdateSettings"))
    .AddHttpClientConfigurationService()
    .AddSingleton<MyConfigurationProviderService>()
    .AddSingleton<MyConfigurationProvider>()
    .AddSingleton<ConnectionStringBuilder>()
    .AddSingleton<INpgsqlConnectionProvider, ConnectionProvider>()
    .AddSingleton<MigrationRunnerFactory>()
    .AddHostedService<ConfigurationsUpdateBackgroundService>()
    .AddHostedService<MigrationsBackgroundService>()
    .AddHostedService<KafkaConsumerBackgroundService<OrderProcessingKey, OrderProcessingValue>>()
    .AddScoped<ExceptionFormattingMiddleware>()
    .AddRepositories()
    .AddServices()
    .AddEndpointsApiExplorer()
    .AddSwagger()
    .AddControllers();

Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

app.UseMiddleware<ExceptionFormattingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.Run();