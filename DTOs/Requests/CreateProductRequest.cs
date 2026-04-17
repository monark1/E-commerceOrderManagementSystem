// DTOs/Requests/CreateProductRequest.cs
// Data the API caller must send when creating a product.
// Validation attributes here prevent invalid data reaching the service layer.

using System.ComponentModel.DataAnnotations;

namespace OrderFlow.API.DTOs.Requests
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // [Range] ensures price is a positive, reasonable number
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 100000")]
        public int StockQuantity { get; set; }
    }
}