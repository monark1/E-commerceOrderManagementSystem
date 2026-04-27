// Controllers/ProductsController.cs
// Thin controller — receives HTTP request, calls service, returns response.
// Zero business logic here. That's the Service's job.

using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.Interfaces.Services;
using OrderFlow.API.Filters;

namespace OrderFlow.API.Controllers
{
    [ApiController]                     // enables automatic model validation
    [Route("api/[controller]")]         // → api/products
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(product);
            // NotFoundException thrown by service → caught by ExceptionMiddleware → 404
        }

        // GET api/products/low-stock?threshold=5
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
        {
            var products = await _productService.GetLowStockProductsAsync(threshold);
            return Ok(products);
        }

        // POST api/products
        [HttpPost]
        [PreventDuplicateRequests]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);
            // 201 Created with Location header pointing to the new resource
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // PUT api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CreateProductRequest request)
        {
            var product = await _productService.UpdateProductAsync(id, request);
            return Ok(product);
        }

        // DELETE api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();     // 204 — success, no body
        }
    }
}