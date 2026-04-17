// Models/OrderItem.cs
// Maps to the "OrderItems" table in MySQL.
// Represents one product line inside an order.
// Example: "3x Nike Shoes at ₹2000 each = ₹6000 subtotal"
// UnitPrice is SNAPSHOTTED at order time — not linked live to Product.Price.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderFlow.API.Models
{
    public class OrderItem
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key → which order this item belongs to
        public int OrderId { get; set; }

        // Foreign Key → which product was ordered
        public int ProductId { get; set; }

        // How many units of this product were ordered
        // Minimum 1 — enforced via DTO validation, not here
        [Required]
        public int Quantity { get; set; }

        // Price per unit AT THE TIME of order placement
        // Critical: if Product.Price changes later, this stays unchanged
        // This ensures order history is always accurate
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Quantity × UnitPrice — computed and stored at order time
        // Stored so we don't recompute on every read
        [Required]    
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // ── Navigation Properties ───
        // Parent order — used with .Include(oi => oi.Order)
        public Order Order { get; set; } = null!;

        // The product that was ordered — used with .ThenInclude(oi => oi.Product)
        // Gives us product name, etc. for response DTOs
        public Product Product { get; set; } = null!;
    }
}