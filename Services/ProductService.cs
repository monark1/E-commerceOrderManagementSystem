// Services/ProductService.cs
// All product business logic lives here.
// Talks to IProductRepository — never touches DbContext directly.

using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;
using OrderFlow.API.Exceptions;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Interfaces.Services;
using OrderFlow.API.Models;

namespace OrderFlow.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repo.GetAllAsync();
            // Map each Product entity → ProductDto (never expose raw entities)
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);

            // If not found, throw — ExceptionMiddleware handles the 404 response
            if (product == null)
                throw new NotFoundException($"Product with Id {id} not found");

            return MapToDto(product);
        }

        public async Task<List<ProductDto>> GetLowStockProductsAsync(int threshold = 5)
        {
            var products = await _repo.GetLowStockAsync(threshold);
            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
        {
            // Map request DTO → entity
            var product = new Product
            {
                Name = request.Name.Trim(),        // trim whitespace
                Description = request.Description?.Trim(),
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(product);
            await _repo.SaveChangesAsync();     // commits INSERT to MySQL

            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, CreateProductRequest request)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with Id {id} not found");

            // Update only the fields from request — leave Id, CreatedAt unchanged
            product.Name = request.Name.Trim();
            product.Description = request.Description?.Trim();
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(product);
            await _repo.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with Id {id} not found");

            await _repo.SoftDeleteAsync(id);
            await _repo.SaveChangesAsync();
        }

        // Private mapper — Product entity → ProductDto
        // Private because only this service needs it
        private static ProductDto MapToDto(Product p) => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            CreatedAt = p.CreatedAt
        };
    }
}