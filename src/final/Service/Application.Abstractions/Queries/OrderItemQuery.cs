namespace Application.Abstractions.Queries;

public record OrderItemQuery(
    IEnumerable<long> OrderIds,
    IEnumerable<long> ProductIds,
    bool IsDeleted,
    int Cursor,
    int PageSize);