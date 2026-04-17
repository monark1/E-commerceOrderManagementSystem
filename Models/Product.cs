// Models/Product.cs
// This class maps directly to the "Products" table in MySQL.
// Each property = one column.
// EF Core reads this class and generates the table via migrations.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderFlow.API.Models
{
    public class Product
    {
        // Primary Key — EF Core auto-detects "Id" as PK
        // AUTO_INCREMENT in MySQL — DB generates this value on insert
        public int Id { get; set; }

        // [Required] tells EF Core → NOT NULL in DB
        // [MaxLength(200)] → VARCHAR(200) in MySQL
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // Nullable string → allows NULL in DB (description is optional)
        [MaxLength(500)]
        public string? Description { get; set; }

        // decimal(18,2) → precise money storage
        // Never use float/double for money — rounding errors
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // How many units are available for purchase
        // Decreases when order is placed, increases when order is cancelled
        [Required]
        public int StockQuantity { get; set; }

        // ── Soft Delete Fields ────────────────────────────────────────────
        // Instead of deleting rows, we hide them using this flag
        // Global Query Filter in DbContext auto-excludes IsDeleted = true rows
        public bool IsDeleted { get; set; } = false;

        // When was this product soft-deleted? null = not deleted
        public DateTime? DeletedAt { get; set; }

        // Who deleted it? Useful for audit trail
        [MaxLength(100)]
        public string? DeletedBy { get; set; }

        // ── Audit Fields ──────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Concurrency Token ─────────────────────────────────────────────
        // EF Core uses this to detect if another request modified this row
        // between our read and our save (optimistic concurrency)
        // MySQL: stored as TIMESTAMP, auto-updated on every row change
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // ── Navigation Property ───────────────────────────────────────────
        // Tells EF Core: one Product appears in many OrderItems
        // Used with .Include(p => p.OrderItems) in LINQ queries
        // Not a DB column — EF Core uses it for JOINs
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}