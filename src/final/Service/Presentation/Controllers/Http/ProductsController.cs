using Application.Ports.Service;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Http;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Создать продукт.
    /// </summary>
    /// <param name="productName">Имя продукта.</param>
    /// /// <param name="productPrice">Цена продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Подтверждение создания продукта.</returns>
    /// <response code="200">Продукт успешно созданы.</response>
    /// <response code="400">Неверные данные продукта.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProductsAsync(
        [FromQuery] string productName,
        [FromQuery] long productPrice,
        CancellationToken cancellationToken)
    {
        var product = new Product(productName, productPrice);
        await _productService.CreateProductsAsync(new Product[] { product }, cancellationToken);
        return Ok("Product successfully created.");
    }
}