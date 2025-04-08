using Application.Abstractions.Dtos;
using Application.Abstractions.Service;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Http;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Создать заказ.
    /// </summary>
    /// <param name="creator">Создатель заказа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Идентификатор созданного заказа.</returns>
    /// <response code="200">Заказ успешно создан.</response>
    /// <response code="400">Неверные параметры запроса.</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrderAsync(
        [FromQuery] string creator,
        CancellationToken cancellationToken)
    {
        var order = new Order(creator);
        await _orderService.CreateOrderAsync(order, cancellationToken);
        return Ok($"Order created: {order.Id}");
    }

    /// <summary>
    /// Добавить продукт в заказ.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="quantity">Количество продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Подтверждение добавления продукта.</returns>
    /// <response code="200">Продукт успешно добавлен.</response>
    /// <response code="400">Неверные параметры запроса.</response>
    /// <response code="404">Заказ или продукт не найдены.</response>
    [HttpPost("{orderId}/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductToOrderAsync(
        [FromRoute] long orderId,
        [FromQuery] long productId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        var item = new OrderItem(orderId, productId, quantity);
        await _orderService.AddProductToOrderAsync(item, cancellationToken);
        return Ok("Product added to order.");
    }

    /// <summary>
    /// Удалить продукт из заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Подтверждение удаления продукта.</returns>
    /// <response code="204">Продукт успешно удалён.</response>
    /// <response code="404">Заказ или продукт не найдены.</response>
    [HttpDelete("{orderId}/products/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProductFromOrderAsync(
        [FromRoute] long orderId,
        [FromRoute] long productId,
        CancellationToken cancellationToken)
    {
        await _orderService.RemoveProductFromOrderAsync(orderId, productId, cancellationToken);
        return Ok("Product removed.");
    }

    /// <summary>
    /// Обновить состояние заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="newState">Новое состояние заказа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Подтверждение обновления состояния.</returns>
    /// <response code="200">Состояние успешно обновлено.</response>
    /// <response code="400">Неверные параметры запроса.</response>
    /// <response code="404">Заказ не найден.</response>
    [HttpPatch("{orderId}/state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStateAsync(
        [FromRoute] long orderId,
        [FromBody] OrderState newState,
        CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderStateAsync(orderId, newState, cancellationToken);
        return Ok("Order state updated.");
    }

    /// <summary>
    /// Получить историю изменений заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cursor">Курсор для постраничного отображения.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список элементов истории заказа.</returns>
    /// <response code="200">История успешно получена.</response>
    /// <response code="404">Заказ не найден.</response>
    [HttpGet("{orderId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderHistoryAsync(
        [FromRoute] long orderId,
        [FromQuery] int cursor,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var historyItemsDto = (await _orderService
                .GetOrderHistoryAsync(orderId, cursor, pageSize, cancellationToken)
                .ToListAsync(cancellationToken))
            .Select(item => item.ToDto())
            .ToList();

        return Ok(historyItemsDto);
    }

    /// <summary>
    /// Получить полную историю изменений заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Полная история заказа.</returns>
    /// <response code="200">История успешно получена.</response>
    /// <response code="404">Заказ не найден.</response>
    [HttpGet("{orderId}/full-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFullOrderHistoryAsync(
        [FromRoute] long orderId,
        CancellationToken cancellationToken)
    {
        var historyItemsDto = (await _orderService
                .GetFullOrderHistoryAsync(orderId, cancellationToken)
                .ToListAsync(cancellationToken))
            .Select(item => item.ToDto())
            .ToList();

        return Ok(historyItemsDto);
    }
}
