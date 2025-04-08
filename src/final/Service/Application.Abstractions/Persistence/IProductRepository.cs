using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;

namespace Application.Abstractions.Persistence;

public interface IProductRepository
{
    // Метод для создания продукта
    public Task<IEnumerable<long>> InsertProductsAsync(
        IReadOnlyCollection<Product> products,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    // Метод для поиска продуктов с фильтрацией и пагинацией
    public IAsyncEnumerable<Product> SearchProductsAsync(
        ProductQuery query,
        CancellationToken cancellationToken);
}