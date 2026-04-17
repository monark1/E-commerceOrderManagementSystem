// Interfaces/Services/ICustomerService.cs
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;

namespace OrderFlow.API.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
        Task<CustomerDto> UpdateCustomerAsync(int id, CreateCustomerRequest request);
        Task DeleteCustomerAsync(int id);
    }
}
