using Application.Abstractions.Persistence;
using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly INpgsqlConnectionProvider _connectionProvider;

    public ProductRepository(INpgsqlConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    // Метод для создания продукта
    public async Task<IEnumerable<long>> InsertProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = """
                           insert into products (product_name, product_price) 
                           select name, price 
                           from unnest(@names, @prices) as source(name, price)
                           returning product_id;
                           """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@names", products.Select(x => x.Name).ToArray()),
                new NpgsqlParameter("@prices", products.Select(x => x.Price).ToArray()),
            },
        };

        var generatedIds = new List<long>();
        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            generatedIds.Add(reader.GetInt64(0)); // Чтение идентификатора из первой колонки (product_id)
        }

        return generatedIds;
    }

    // Метод для поиска продуктов с фильтрацией и пагинацией
    public async IAsyncEnumerable<Product> SearchProductsAsync(
        ProductQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        const string sqlQuery = """
                                  select * from products
                                  where
                                    (product_id > @cursor)
                                    and (cardinality(@ids) = 0 or product_id = any (@ids))
                                    and (@name_pattern is null or product_name like @name_pattern)
                                    and (@min_price is null or product_price >= @min_price)
                                    and (@max_price is null or product_price <= @max_price)
                                  order by product_id
                                  limit @page_size;
                                  """;

        using var command = new NpgsqlCommand(sqlQuery, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("@ids", query.Ids),
                new NpgsqlParameter("@name_pattern", '%' + query.NameSubstring + '%'),
                new NpgsqlParameter("@min_price", query.MinPrice ?? (object)DBNull.Value) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Money },
                new NpgsqlParameter("@max_price", query.MaxPrice ?? (object)DBNull.Value) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Money },
                new NpgsqlParameter("@cursor", query.Cursor),
                new NpgsqlParameter("@page_size", query.PageSize),
            },
        };

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            yield return new Product(
                reader.GetInt64(reader.GetOrdinal("product_id")),
                reader.GetString(reader.GetOrdinal("product_name")),
                reader.GetDecimal(reader.GetOrdinal("product_price")));
        }
    }
}
