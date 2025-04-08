namespace Presentation.Kafka.Consumer;

public class KafkaConsumerOptions
{
    public string BootstrapServers { get; set; } = "localhost:8001";

    public string Topic { get; set; } = "order_processing";

    public string GroupId { get; set; } = "order-service";

    public int BatchSize { get; set; } = 10;

    public KafkaConsumerOptions() { }

    public KafkaConsumerOptions(string bootstrapServers, string topic, string groupId, int batchSize)
    {
        BootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
        BatchSize = batchSize;
    }
}