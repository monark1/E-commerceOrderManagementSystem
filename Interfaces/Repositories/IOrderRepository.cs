using OrderFlow.API.Enums;
using OrderFlow.API.Models;

namespace OrderFlow.API.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Models.Order?> GetByIdAsync(int id);
        Task<Models.Order?> GetWithDetailsAsync(int id);
        Task<List<Order>> GetAllAsync();
        Task DetachAllAsync();
        Task<List<Models.Order>> GetCustomerOrdersAsync(int customerId, OrderStatus? status);
        Task CreateAsync(Models.Order order);
        Task UpdateAsync(Models.Order order);
        Task SoftDeleteAsync(int id, string deletedBy = "system");
        Task SaveChangesAsync();
    }
}