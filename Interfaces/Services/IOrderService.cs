// Interfaces/Services/IOrderService.cs
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;
using OrderFlow.API.Enums;

namespace OrderFlow.API.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<List<OrderDto>> GetCustomerOrdersAsync(int customerId, OrderStatus? status);
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task CancelOrderAsync(int orderId);
        Task DeleteOrderAsync(int id);
    }
}