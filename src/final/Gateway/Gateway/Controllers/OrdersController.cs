using Gateway.Grpc;
using Grpc.Core;
using GrpcGeneratedClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly GrpcGeneratedClasses.Orders.OrdersClient _grpcClient;

    public OrdersController(GrpcGeneratedClasses.Orders.OrdersClient grpcClient)
    {
        _grpcClient = grpcClient;
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrderAsync(
        [FromQuery] string creator,
        CancellationToken cancellationToken)
    {
        var request = new CreateOrderRequest { CreatedBy = creator };
        CreateOrderResponse response = await _grpcClient.CreateOrderAsync(request, cancellationToken: cancellationToken);
        return Ok(new { OrderId = response.Id });
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductToOrderAsync(
        [FromRoute] long orderId,
        [FromQuery] long productId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        var request = new AddProductToOrderRequest
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };
        AddProductToOrderResponse response = await _grpcClient.AddProductToOrderAsync(request, cancellationToken: cancellationToken);
        return Ok(response.Message);
    }

    /// <summary>
    /// Удалить продукт из заказа.
    /// </summary>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
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
        var request = new RemoveProductFromOrderRequest
        {
            OrderId = orderId,
            ProductId = productId,
        };
        RemoveProductFromOrderResponse response = await _grpcClient.RemoveProductFromOrderAsync(request, cancellationToken: cancellationToken);
        return Ok(response.Message);
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStateAsync(
        [FromRoute] long orderId,
        [FromBody] string newState,
        CancellationToken cancellationToken)
    {
        var request = new UpdateOrderStateRequest { OrderId = orderId, NewState = newState.ToGrpcOrderState() };
        UpdateOrderStateResponse response = await _grpcClient.UpdateOrderStateAsync(request, cancellationToken: cancellationToken);
        return Ok(response.Message);
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
    [ProducesResponseType(typeof(IEnumerable<OrderHistoryEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderHistoryAsync(
        [FromRoute] long orderId,
        [FromQuery] int cursor,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var request = new GetOrderHistoryRequest { OrderId = orderId, Cursor = cursor, PageSize = pageSize };

        AsyncServerStreamingCall<OrderHistoryEntry> responseStream = _grpcClient.GetOrderHistory(request, cancellationToken: cancellationToken);

        var history = new List<object>();
        await foreach (OrderHistoryEntry entry in responseStream.ResponseStream.ReadAllAsync(cancellationToken))
        {
            history.Add(entry);
        }

        return Ok(history);
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
    [ProducesResponseType(typeof(IEnumerable<OrderHistoryEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFullOrderHistoryAsync(
        [FromRoute] long orderId,
        CancellationToken cancellationToken)
    {
        var request = new GetFullOrderHistoryRequest { OrderId = orderId };
        AsyncServerStreamingCall<OrderHistoryEntry> responseStream = _grpcClient.GetFullOrderHistory(request, cancellationToken: cancellationToken);

        var history = new List<object>();
        await foreach (OrderHistoryEntry entry in responseStream.ResponseStream.ReadAllAsync(cancellationToken))
        {
            history.Add(entry);
        }

        return Ok(history);
    }
}