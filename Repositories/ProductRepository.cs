// Repositories/ProductRepository.cs
// Handles all DB operations for Products.
// ONLY queries and data access here — zero business logic.

using Microsoft.EntityFrameworkCore;
using OrderFlow.API.Data;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Models;

namespace OrderFlow.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        // DbContext injected via constructor — DI handles this
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all products — Global Query Filter auto-excludes IsDeleted = true
        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)       // alphabetical order
                .ToListAsync();
        }

        // Get single product by Id — returns null if not found
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Check stock before placing order — used in OrderService validation
        public async Task<bool> HasSufficientStockAsync(int productId, int qty)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == productId && p.StockQuantity >= qty);
        }

        // Get products running low on stock — default threshold is 5 units
        public async Task<List<Product>> GetLowStockAsync(int threshold = 5)
        {
            return await _context.Products
                .Where(p => p.StockQuantity < threshold)
                .OrderBy(p => p.StockQuantity)  // lowest stock first
                .ToListAsync();
        }

        // Add new product to DB — EF Core tracks it as Added state
        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        // Update existing product — EF Core tracks changes automatically
        public Task UpdateAsync(Product product)
        {
            // Mark entity as Modified — EF Core will generate UPDATE SQL
            _context.Products.Update(product);
            return Task.CompletedTask;  // no async needed for Update()
        }

        // Soft delete — sets IsDeleted flag, never removes the actual row
        public async Task SoftDeleteAsync(int id, string deletedBy = "system")
        {
            var product = await GetByIdAsync(id);
            if (product == null) return;

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.DeletedBy = deletedBy;
        }

        // Commit all pending changes to MySQL in one transaction
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}