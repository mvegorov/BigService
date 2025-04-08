using Gateway.Grpc;
using GrpcGeneratedClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly Products.ProductsClient _grpcClient;

    public ProductsController(Products.ProductsClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    /// <summary>
    /// Создать продукт.
    /// </summary>
    /// <param name="productName">Имя продукта.</param>
    /// /// <param name="productPrice">Цена продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Подтверждение создания продукта.</returns>
    /// <response code="200">Продукт успешно создан.</response>
    /// <response code="400">Неверные данные продукта.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProductsAsync(
        [FromQuery] string productName,
        [FromQuery] decimal productPrice,
        CancellationToken cancellationToken)
    {
        var request = new CreateProductRequest { Name = productName, Price = productPrice.ToGrpcMoney() };
        CreateProductResponse response = await _grpcClient.CreateProductAsync(request, cancellationToken: cancellationToken);
        return Ok(new { ProductId = response.Id });
    }
}