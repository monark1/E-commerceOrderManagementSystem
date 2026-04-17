// Repositories/OrderRepository.cs

using Microsoft.EntityFrameworkCore;
using OrderFlow.API.Data;
using OrderFlow.API.Enums;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Models;

namespace OrderFlow.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // Basic fetch — no related data loaded
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Full fetch — loads Customer + OrderItems + Product names
        // Used when returning order details or processing cancellation
        public async Task<Order?> GetWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)               // load customer info
                .Include(o => o.OrderItems)             // load all line items
                    .ThenInclude(oi => oi.Product)      // load product for each item
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // My Orders feature — all orders for a customer with optional status filter
        public async Task<List<Order>> GetCustomerOrdersAsync(
            int customerId, OrderStatus? status)
        {
            // Build query step by step — conditional chaining
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .AsQueryable();     // keeps it as IQueryable so we can add more filters

            // Only add status filter if caller provided one
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            // Latest order appears first
            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(int id, string deletedBy = "system")
        {
            var order = await GetByIdAsync(id);
            if (order == null) return;

            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            order.DeletedBy = deletedBy;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}