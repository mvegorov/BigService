using Domain.Entities;

namespace Application.Abstractions.Queries;

public record OrderQuery(
    IEnumerable<long> Ids,
    IEnumerable<OrderState> States,
    string? Creator = null,
    int Cursor = 0,
    int PageSize = 1);