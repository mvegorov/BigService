#pragma warning disable IDE0005
using Kafka.Abstractions.Produser;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Kafka.Consumer;
using Presentation.Kafka.Producer;

namespace Presentation.Kafka;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafkaProducer<TKey, TValue>(this IServiceCollection services)
    {
        services.AddSingleton<IKafkaProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();
        return services;
    }

    public static IServiceCollection AddKafkaConsumer<TKey, TValue>(this IServiceCollection services)
    {
        services.AddHostedService<KafkaConsumerBackgroundService<TKey, TValue>>();
        return services;
    }
}