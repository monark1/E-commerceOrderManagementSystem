// Data/AppDbContext.cs
// The heart of EF Core. This class:
//   1. Connects to MySQL
//   2. Represents all DB tables as DbSet<T> properties
//   3. Configures relationships, constraints, and query filters
//   4. Handles change tracking and SaveChangesAsync()

using Microsoft.EntityFrameworkCore;
using OrderFlow.API.Models;

namespace OrderFlow.API.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor — ASP.NET Core DI injects the options (connection string etc.)
        // We pass them to the base DbContext class
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ── DbSets ───
        // Each DbSet<T> = one table in MySQL
        // Used in repositories: _context.Products.Where(...)
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // OnModelCreating runs once at startup
        // This is where we configure things that can't be done via attributes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Always call base first — runs EF Core's own configuration
            base.OnModelCreating(modelBuilder);

            // ── GLOBAL QUERY FILTERS ───
            // These filters are automatically appended to EVERY query on these tables
            // You never need to write .Where(p => !p.IsDeleted) manually
            // Example: _context.Products.ToListAsync()
            //   → SQL: SELECT * FROM Products WHERE IsDeleted = 0
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Order>()
                .HasQueryFilter(o => !o.IsDeleted);

            // ── UNIQUE CONSTRAINTS ───
            // Ensures no two customers can have the same email
            // EF Core creates a UNIQUE INDEX on this column in MySQL
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // ── RELATIONSHIPS ───

            // Customer → Orders (One customer, many orders)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)            // Order has ONE Customer
                .WithMany(c => c.Orders)            // Customer has MANY Orders
                .HasForeignKey(o => o.CustomerId)   // FK column in Orders table
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting customer
                                                    // if they have orders

            // Order → OrderItems (One order, many line items)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);  // delete items when order deleted

            // Product → OrderItems (One product, many order items)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting a product
                                                    // that exists in orders

            // ── CONCURRENCY TOKEN ───
            // Tells EF Core to use RowVersion for optimistic concurrency on Product
            // When two requests try to update stock simultaneously,
            // EF Core detects the conflict and throws DbUpdateConcurrencyException
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // ── DECIMAL PRECISION ────
            // MySQL requires explicit decimal type — avoids silent rounding
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Subtotal)
                .HasColumnType("decimal(18,2)");
        }
    }
}