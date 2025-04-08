namespace Presentation.Kafka.Producer;

public class KafkaProducerOptions
{
    public string BootstrapServers { get; set; } = "localhost:8001";

    public string Topic { get; set; } = "order_creation";

    public KafkaProducerOptions() { }

    public KafkaProducerOptions(string bootstrapServer, string topic)
    {
        BootstrapServers = bootstrapServer ?? throw new ArgumentNullException(nameof(bootstrapServer));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
    }
}