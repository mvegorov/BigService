namespace Application.Abstractions.Queries;

public record ProductQuery(
    IEnumerable<long> Ids,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? NameSubstring,
    int Cursor,
    int PageSize);