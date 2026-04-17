// DTOs/Responses/ProductDto.cs
// What the API returns when someone requests product data.
// Never return raw entity (Product) — always map to DTO.
// Reason: hides internal fields like RowVersion, IsDeleted from API callers.

namespace OrderFlow.API.DTOs.Responses
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }

        // Low stock warning — business logic visible in response
        // If stock < 5, frontend can show a "Low Stock" badge
        public bool IsLowStock => StockQuantity > 0 && StockQuantity < 5;

        // Out of stock flag — frontend can disable "Add to Cart"
        public bool IsOutOfStock => StockQuantity == 0;
    }
}