using Npgsql;

namespace Application.Abstractions.Persistence;

public interface INpgsqlConnectionProvider
{
    public ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken);
}