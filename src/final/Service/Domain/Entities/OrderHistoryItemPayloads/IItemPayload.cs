using System.Text.Json.Serialization;

namespace Domain.Entities.OrderHistoryItemPayloads;

[JsonDerivedType(typeof(ItemAddedPayload), typeDiscriminator: nameof(ItemAddedPayload))]
[JsonDerivedType(typeof(ItemRemovedPayload), typeDiscriminator: nameof(ItemRemovedPayload))]
[JsonDerivedType(typeof(OrderCreatedPayload), typeDiscriminator: nameof(OrderCreatedPayload))]
[JsonDerivedType(typeof(StateChangedPayload), typeDiscriminator: nameof(StateChangedPayload))]
public interface IItemPayload
{
}
