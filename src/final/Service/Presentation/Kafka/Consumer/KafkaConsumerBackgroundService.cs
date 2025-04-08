#pragma warning disable IDE0005
using Confluent.Kafka;
using Kafka.Abstractions.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Presentation.Kafka.Consumer;

public class KafkaConsumerBackgroundService<TKey, TValue> : BackgroundService
{
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaConsumerOptions _options;

    public KafkaConsumerBackgroundService(IOptions<KafkaConsumerOptions> options, IServiceScopeFactory scopeFactory)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        _consumer = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(new ProtobufSerializer<TKey>())
            .SetValueDeserializer(new ProtobufSerializer<TValue>())
            .Build();

        _consumer.Subscribe(_options.Topic);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = new List<ConsumeResult<TKey, TValue>>();

            try
            {
                for (int i = 0; i < _options.BatchSize; i++)
                {
                    ConsumeResult<TKey, TValue> result = _consumer.Consume(stoppingToken);
                    if (result != null)
                    {
                        messages.Add(result);
                    }
                }

                if (messages.Count > 0)
                {
                    using (IServiceScope scope = _scopeFactory.CreateScope())
                    {
                        IKafkaMessageHandler<TKey, TValue> handler = scope.ServiceProvider.GetRequiredService<IKafkaMessageHandler<TKey, TValue>>();
                        await handler.HandleAsync(messages, stoppingToken);
                    }

                    _consumer.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"consumer error: {ex.Message}");
            }
        }
    }
}
