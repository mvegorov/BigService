namespace Kafka.Abstractions.Consumer;

public interface IKafkaConsumer
{
    void Subscribe(string topic);

    void StartConsuming(CancellationToken cancellationToken);
}