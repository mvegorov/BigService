﻿namespace Kafka.Abstractions.Produser;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(TKey key, TValue value, CancellationToken cancellationToken = default);
}