// Interfaces/Repositories/IProductRepository.cs
// Contract for all product DB operations.
// Service layer depends on this interface — not the concrete class.
// This is the D in SOLID (Dependency Inversion).

namespace OrderFlow.API.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<List<Models.Product>> GetAllAsync();
        Task<Models.Product?> GetByIdAsync(int id);
        Task<bool> HasSufficientStockAsync(int productId, int qty);
        Task<List<Models.Product>> GetLowStockAsync(int threshold = 5);
        Task CreateAsync(Models.Product product);
        Task UpdateAsync(Models.Product product);
        Task SoftDeleteAsync(int id, string deletedBy = "system");
        Task SaveChangesAsync();
    }
}