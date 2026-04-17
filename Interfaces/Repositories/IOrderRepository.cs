// Interfaces/Repositories/IOrderRepository.cs

using OrderFlow.API.Enums;

namespace OrderFlow.API.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Models.Order?> GetByIdAsync(int id);
        Task<Models.Order?> GetWithDetailsAsync(int id);
        Task<List<Models.Order>> GetCustomerOrdersAsync(int customerId, OrderStatus? status);
        Task CreateAsync(Models.Order order);
        Task UpdateAsync(Models.Order order);
        Task SoftDeleteAsync(int id, string deletedBy = "system");
        Task SaveChangesAsync();
    }
}