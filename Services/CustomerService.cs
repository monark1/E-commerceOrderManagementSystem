// Services/CustomerService.cs

using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;
using OrderFlow.API.Exceptions;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Interfaces.Services;
using OrderFlow.API.Models;

namespace OrderFlow.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;

        public CustomerService(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repo.GetAllAsync();
            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _repo.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException($"Customer with Id {id} not found");

            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
        {
            // Business rule — no duplicate emails
            var emailTaken = await _repo.EmailExistsAsync(request.Email);
            if (emailTaken)
                throw new BadRequestException(
                    $"Email '{request.Email}' is already registered");

            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLower(), // normalize email
                Phone = request.Phone?.Trim(),
                Address = request.Address?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(customer);
            await _repo.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(
            int id, CreateCustomerRequest request)
        {
            var customer = await _repo.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException($"Customer with Id {id} not found");

            // If email is changing, check new email isn't taken by someone else
            if (!customer.Email.Equals(request.Email,
                StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _repo.EmailExistsAsync(request.Email);
                if (emailTaken)
                    throw new BadRequestException(
                        $"Email '{request.Email}' is already in use");
            }

            customer.Name = request.Name.Trim();
            customer.Email = request.Email.Trim().ToLower();
            customer.Phone = request.Phone?.Trim();
            customer.Address = request.Address?.Trim();

            await _repo.UpdateAsync(customer);
            await _repo.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _repo.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException($"Customer with Id {id} not found");

            await _repo.SoftDeleteAsync(id);
            await _repo.SaveChangesAsync();
        }

        private static CustomerDto MapToDto(Customer c) => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            CreatedAt = c.CreatedAt
        };
    }
}