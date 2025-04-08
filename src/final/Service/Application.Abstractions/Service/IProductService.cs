using Domain.Entities;

namespace Application.Ports.Service;

public interface IProductService
{
    public Task CreateProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);
}