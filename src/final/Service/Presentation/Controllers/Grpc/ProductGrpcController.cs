using Application.Ports.Service;
using Domain.Entities;
using Grpc.Core;
using GrpcGeneratedClasses;

namespace Presentation.Controllers.Grpc;

public class ProductGrpcController : Products.ProductsBase
{
    private readonly IProductService _productService;

    public ProductGrpcController(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = new Product(request.Name, request.Price.ToDecimal());
        await _productService.CreateProductsAsync(new Product[] { product }, context.CancellationToken);

        if (product.Id is null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }

        return new CreateProductResponse { Id = product.Id.Value };
    }
}