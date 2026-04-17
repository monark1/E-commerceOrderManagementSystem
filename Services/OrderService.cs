// Services/OrderService.cs
// Most complex service — handles order creation with stock validation,
// concurrency handling, order cancellation with stock restore,
// and status transitions.

using Microsoft.EntityFrameworkCore;
using OrderFlow.API.Data;
using OrderFlow.API.DTOs.Requests;
using OrderFlow.API.DTOs.Responses;
using OrderFlow.API.Enums;
using OrderFlow.API.Exceptions;
using OrderFlow.API.Interfaces.Repositories;
using OrderFlow.API.Interfaces.Services;
using OrderFlow.API.Models;

namespace OrderFlow.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly AppDbContext _context;   // needed for transactions

        public OrderService(
            IOrderRepository orderRepo,
            IProductRepository productRepo,
            ICustomerRepository customerRepo,
            AppDbContext context)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _customerRepo = customerRepo;
            _context = context;
        }

        public async Task<OrderDto> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepo.GetWithDetailsAsync(id);
            if (order == null)
                throw new NotFoundException($"Order with Id {id} not found");

            return MapToDto(order);
        }

        // My Orders — returns all orders for a customer
        public async Task<List<OrderDto>> GetCustomerOrdersAsync(
            int customerId, OrderStatus? status)
        {
            // Verify customer exists first
            var customer = await _customerRepo.GetByIdAsync(customerId);
            if (customer == null)
                throw new NotFoundException($"Customer with Id {customerId} not found");

            var orders = await _orderRepo.GetCustomerOrdersAsync(customerId, status);

            // Empty list is valid — customer exists but has no orders yet
            return orders.Select(MapToDto).ToList();
        }

        // Order creation with concurrency handling
        public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
        {
            // Retry loop — handles stock race conditions (max 3 attempts)
            // This is the MINIMAL concurrency approach Aman asked for
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    // ── Step 1: Validate customer ───
                    var customer = await _customerRepo.GetByIdAsync(request.CustomerId);
                    if (customer == null)
                        throw new NotFoundException(
                            $"Customer {request.CustomerId} not found");

                    // ── Step 2: Validate ALL items have stock ───
                    // Do this BEFORE deducting anything
                    // If item 3 of 5 fails, we haven't touched items 1 and 2
                    foreach (var item in request.Items)
                    {
                        var hasStock = await _productRepo
                            .HasSufficientStockAsync(item.ProductId, item.Quantity);

                        if (!hasStock)
                        {
                            // Fetch product name for a helpful error message
                            var p = await _productRepo.GetByIdAsync(item.ProductId);
                            var name = p?.Name ?? $"ProductId {item.ProductId}";
                            throw new BadRequestException(
                                $"Insufficient stock for '{name}'. " +
                                $"Requested: {item.Quantity}, " +
                                $"Available: {p?.StockQuantity ?? 0}");
                        }
                    }

                    // ── Step 3: Build order items + deduct stock ───
                    var orderItems = new List<OrderItem>();
                    foreach (var item in request.Items)
                    {
                        var product = await _productRepo.GetByIdAsync(item.ProductId);

                        // Deduct stock — EF Core tracks this change
                        product!.StockQuantity -= item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;

                        orderItems.Add(new OrderItem
                        {
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,                  // snapshot
                            Subtotal = item.Quantity * product.Price   // compute
                        });
                    }

                    // ── Step 4: Create the order ───
                    var order = new Order
                    {
                        CustomerId = request.CustomerId,
                        Status = OrderStatus.Pending,
                        TotalAmount = orderItems.Sum(oi => oi.Subtotal),
                        ShippingAddress = request.ShippingAddress?.Trim()
                                          ?? customer.Address,      // fallback to customer's address
                        Notes = request.Notes?.Trim(),
                        OrderItems = orderItems,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderRepo.CreateAsync(order);

                    // ── Step 5: Commit ────
                    // SaveChangesAsync checks RowVersion — throws if stock was
                    // modified by another request between our read and save
                    await _context.SaveChangesAsync();

                    // Re-fetch with full details for response
                    var created = await _orderRepo.GetWithDetailsAsync(order.Id);
                    return MapToDto(created!);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Another request modified the product's RowVersion
                    // Our data is now stale — increment retry and try again
                    retryCount++;

                    if (retryCount >= maxRetries)
                        throw new ConcurrencyException(
                            "Could not complete order due to high demand. " +
                            "Please try again in a moment.");

                    // Detach ALL tracked entities so EF Core re-reads fresh data
                    // on the next attempt — without this, it would use cached stale data
                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }

            throw new ConcurrencyException("Order could not be processed. Please retry.");
        }

        // Status update — moves order forward in lifecycle
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new NotFoundException($"Order {orderId} not found");

            // Business rule — status can only move forward, not backward
            // (except Cancelled which is handled separately)
            if (newStatus == OrderStatus.Cancelled)
                throw new BadRequestException(
                    "Use the cancel endpoint to cancel an order");

            if ((int)newStatus <= (int)order.Status)
                throw new BadRequestException(
                    $"Cannot move order from '{order.Status}' to '{newStatus}'");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepo.UpdateAsync(order);
            await _context.SaveChangesAsync();
        }

        // Cancel order + restore stock
        public async Task CancelOrderAsync(int orderId)
        {
            // Need full details to access OrderItems for stock restore
            var order = await _orderRepo.GetWithDetailsAsync(orderId);
            if (order == null)
                throw new NotFoundException($"Order {orderId} not found");

            // Business rule — can only cancel Pending or Confirmed orders
            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
                throw new BadRequestException(
                    $"Order in '{order.Status}' status cannot be cancelled. " +
                    "Only Pending or Confirmed orders can be cancelled.");

            // Restore stock for every item in the order
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity; // restore
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException($"Order {id} not found");

            await _orderRepo.SoftDeleteAsync(id);
            await _context.SaveChangesAsync();
        }

        // Maps Order entity → OrderDto for API response
        private static OrderDto MapToDto(Order o) => new OrderDto
        {
            OrderId = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.Name ?? string.Empty,
            Status = o.Status.ToString(),   // "Pending" not 0
            TotalAmount = o.TotalAmount,
            ShippingAddress = o.ShippingAddress,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = o.OrderItems?.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                // If product was soft-deleted, show "[Removed]" gracefully
                ProductName = oi.Product?.Name ?? "[Product Removed]",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal
            }).ToList() ?? new List<OrderItemDto>()
        };
    }
}