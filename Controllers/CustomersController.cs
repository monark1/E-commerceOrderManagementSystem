// Controllers/CustomersController.cs

using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.Enums;
using OrderFlow.API.Interfaces.Services;

namespace OrderFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]     // → api/customers
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public CustomersController(
            ICustomerService customerService,
            IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        // GET api/customers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET api/customers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            return Ok(customer);
        }

        // GET api/customers/5/orders?status=Pending
        // My Orders feature — all orders for this customer
        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetOrders(
            int id,
            [FromQuery] OrderStatus? status = null)     // optional filter
        {
            var orders = await _orderService.GetCustomerOrdersAsync(id, status);
            return Ok(orders);
        }

        // POST api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            var customer = await _customerService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        // PUT api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CreateCustomerRequest request)
        {
            var customer = await _customerService.UpdateCustomerAsync(id, request);
            return Ok(customer);
        }

        // DELETE api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}