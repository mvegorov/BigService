using Microsoft.AspNetCore.Mvc;
using Orders.ProcessingService.Contracts;

namespace Gateway.Controllers;

[ApiController]
[Route("api/orders")]
public class Lab5ToolsController : ControllerBase
{
    private readonly OrderService.OrderServiceClient _orderServiceClient;

    public Lab5ToolsController(OrderService.OrderServiceClient orderServiceClient)
    {
        _orderServiceClient = orderServiceClient;
    }

    [HttpPut("{orderId}/approval")]
    public async Task<IActionResult> ApproveOrder(long orderId, [FromBody] ApproveOrderRequest request)
    {
        request.OrderId = orderId;
        await _orderServiceClient.ApproveOrderAsync(request);
        return NoContent();
    }

    [HttpPut("{orderId}/packing/start")]
    public async Task<IActionResult> StartOrderPacking(long orderId, [FromBody] StartOrderPackingRequest request)
    {
        request.OrderId = orderId;
        await _orderServiceClient.StartOrderPackingAsync(request);
        return NoContent();
    }

    [HttpPut("{orderId}/packing/finish")]
    public async Task<IActionResult> FinishOrderPacking(long orderId, [FromBody] FinishOrderPackingRequest request)
    {
        request.OrderId = orderId;
        await _orderServiceClient.FinishOrderPackingAsync(request);
        return NoContent();
    }

    [HttpPut("{orderId}/delivery/start")]
    public async Task<IActionResult> StartOrderDelivery(long orderId, [FromBody] StartOrderDeliveryRequest request)
    {
        request.OrderId = orderId;
        await _orderServiceClient.StartOrderDeliveryAsync(request);
        return NoContent();
    }

    [HttpPut("{orderId}/delivery/finish")]
    public async Task<IActionResult> FinishOrderDelivery(long orderId, [FromBody] FinishOrderDeliveryRequest request)
    {
        request.OrderId = orderId;
        await _orderServiceClient.FinishOrderDeliveryAsync(request);
        return NoContent();
    }
}
