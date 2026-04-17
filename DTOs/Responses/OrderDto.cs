// DTOs/Responses/OrderDto.cs
// What the API returns for order data.
// Includes a nested list of OrderItemDto — full order detail in one response.

namespace OrderFlow.API.DTOs.Responses
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        // Status returned as string ("Pending") not int (0) — readable for clients
        public string Status { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Total item count — useful for order summary cards
        public int ItemCount => Items.Count;

        // Full list of products in this order
        public List<OrderItemDto> Items { get; set; } = new();
    }

    // One line item inside an order response
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}