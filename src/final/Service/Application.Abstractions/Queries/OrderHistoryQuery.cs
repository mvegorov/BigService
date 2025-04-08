using Domain.Entities;

namespace Application.Abstractions.Queries;

public record OrderHistoryQuery(
    IEnumerable<long> OrderIds,
    IEnumerable<OrderHistoryItemKind> ItemKinds,
    long Cursor,
    int PageSize);