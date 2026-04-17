// Interfaces/Repositories/ICustomerRepository.cs

namespace OrderFlow.API.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Models.Customer>> GetAllAsync();
        Task<Models.Customer?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task CreateAsync(Models.Customer customer);
        Task UpdateAsync(Models.Customer customer);
        Task SoftDeleteAsync(int id, string deletedBy = "system");
        Task SaveChangesAsync();
    }
}