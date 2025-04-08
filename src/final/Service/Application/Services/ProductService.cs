using Application.Abstractions.Persistence;
using Application.Ports.Service;
using Domain.Entities;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task CreateProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        IEnumerable<long> generatedIds = await _productRepository.InsertProductsAsync(products, cancellationToken);

        using IEnumerator<long> idEnumerator = generatedIds.GetEnumerator();
        foreach (Product product in products)
        {
            if (!idEnumerator.MoveNext())
                throw new InvalidOperationException("Product cannot be created.");

            product.Id = idEnumerator.Current;
        }
    }
}