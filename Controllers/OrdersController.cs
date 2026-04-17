// Controllers/OrdersController.cs

using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.Enums;
using OrderFlow.API.Interfaces.Services;

namespace OrderFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]     // → api/orders
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET api/orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
        }

        // PATCH api/orders/5/status
        // Move order forward: Pending → Confirmed → Shipped → Delivered
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] OrderStatus newStatus)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus);
            return NoContent();
        }

        // PATCH api/orders/5/cancel
        // Cancel order + restore stock
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _orderService.CancelOrderAsync(id);
            return NoContent();
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}