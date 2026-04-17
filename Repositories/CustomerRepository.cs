// Repositories/CustomerRepository.cs

using Microsoft.EntityFrameworkCore;
using OrderFlow.API.Data;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Models;

namespace OrderFlow.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Check if email is already registered — for duplicate prevention
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email);
        }

        public async Task CreateAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(int id, string deletedBy = "system")
        {
            var customer = await GetByIdAsync(id);
            if (customer == null) return;

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = deletedBy;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}