#pragma warning disable IDE0005
using Confluent.Kafka;
using Kafka.Abstractions.Produser;
using Microsoft.Extensions.Options;

namespace Presentation.Kafka.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IDisposable
{
    private readonly IProducer<TKey, TValue> _producer;

    private readonly string _topic;

    public KafkaProducer(IOptions<KafkaProducerOptions> options)
    {
        var config = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers };
        _topic = options.Value.Topic;
        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(new ProtobufSerializer<TKey>())
            .SetValueSerializer(new ProtobufSerializer<TValue>())
            .Build();
    }

    public async Task ProduceAsync(TKey key, TValue value, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync(_topic, new Message<TKey, TValue> { Key = key, Value = value }, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose() => _producer.Dispose();
}
