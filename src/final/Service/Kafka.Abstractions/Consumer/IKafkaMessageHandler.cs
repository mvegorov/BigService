using Confluent.Kafka;

namespace Kafka.Abstractions.Consumer;

public interface IKafkaMessageHandler<TKey, TValue>
{
    Task HandleAsync(IReadOnlyCollection<ConsumeResult<TKey, TValue>> messages, CancellationToken cancellationToken);
}