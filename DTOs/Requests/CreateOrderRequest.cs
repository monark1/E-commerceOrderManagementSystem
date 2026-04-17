// DTOs/Requests/CreateOrderRequest.cs
// Data required to place a new order.
// Contains customer ID and a list of products with quantities.

using System.ComponentModel.DataAnnotations;

namespace OrderFlow.API.DTOs.Requests
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "CustomerId is required")]
        public int CustomerId { get; set; }

        // Must contain at least one item — can't place an empty order
        [Required]
        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public List<OrderItemRequest> Items { get; set; } = new();

        [MaxLength(400)]
        public string? ShippingAddress { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    // Nested DTO — one line item inside the order request
    public class OrderItemRequest
    {
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
    }
}