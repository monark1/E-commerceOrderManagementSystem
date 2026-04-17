// Interfaces/Services/IProductService.cs
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;

namespace OrderFlow.API.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<List<ProductDto>> GetLowStockProductsAsync(int threshold = 5);
        Task<ProductDto> CreateProductAsync(CreateProductRequest request);
        Task<ProductDto> UpdateProductAsync(int id, CreateProductRequest request);
        Task DeleteProductAsync(int id);
    }
}