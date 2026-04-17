// Models/Order.cs
// Maps to the "Orders" table in MySQL.
// One order belongs to one customer and contains many OrderItems.
// TotalAmount is stored (not recomputed) — snapshot at order creation time.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderFlow.API.Enums;

namespace OrderFlow.API.Models
{
    public class Order
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Key → links this order to a specific customer
        // EF Core creates the actual FK constraint in MySQL
        public int CustomerId { get; set; }

        // Order lifecycle status — stored as int (0,1,2,3,4) in DB
        // Default is Pending when order is first created
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Total cost of the entire order = SUM of all OrderItem.Subtotals
        // Stored at order creation time — does NOT change if product prices change later
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Where to deliver the order (optional — can inherit from customer)
        [MaxLength(400)]
        public string? ShippingAddress { get; set; }

        // Any special instructions from the customer
        [MaxLength(500)]
        public string? Notes { get; set; }

        // ── Soft Delete Fields ───
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [MaxLength(100)]
        public string? DeletedBy { get; set; }

        // ── Audit Fields ───
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ── Navigation Properties ───
        // Reference to the parent Customer — used with .Include(o => o.Customer)
        // null! means "I guarantee EF Core will populate this, trust me"
        public Customer Customer { get; set; } = null!;

        // Collection of line items in this order
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}