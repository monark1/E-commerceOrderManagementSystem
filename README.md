# E-Commerce Order Management System

> E-Commerce Order Management System — built as part of internship evaluation at Windmöller & Hölscher India Pvt. Ltd.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Key Features](#key-features)
- [Data Lifecycle — Smart Deletion](#data-lifecycle--smart-deletion)
- [Console Client](#console-client)


---

## Overview

OrderFlow is a RESTful Web API that manages the core operations of an e-commerce platform — **products**, **sellers**, **customers**, and **orders**. It is accompanied by a C# console application that demonstrates the full end-to-end workflow from two perspectives: a customer shopping and a seller managing inventory.

The project was designed with a strong focus on clean architecture, SOLID principles, and real-world engineering practices including optimistic concurrency handling, smart data lifecycle management, soft delete with automated cleanup, server-side seller ownership enforcement, and idempotency protection.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8 Web API |
| Database | MySQL 8 |
| ORM | Entity Framework Core 8 (Pomelo provider) |
| API Docs | Swagger / Swashbuckle |
| Caching | IMemoryCache (built-in) |
| Background Jobs | .NET IHostedService (BackgroundService) |
| Console Client | .NET 8 Console App |

---

## Architecture

```
HTTP Request
     ↓
ExceptionMiddleware      ← wraps everything, catches all errors
     ↓
Controller               ← receives request, calls service, returns response
     ↓
Service                  ← all business rules live here
     ↓
Repository               ← all database queries live here
     ↓
AppDbContext             ← EF Core gateway to MySQL
     ↓
MySQL
```

**Design Patterns used:**
- Repository Pattern — isolates all DB logic behind interfaces
- Service Layer Pattern — centralizes all business rules
- Middleware Pipeline Pattern — global exception handling in one place
- Optimistic Concurrency Pattern — handles simultaneous stock deductions via RowVersion
- Data Lifecycle Pattern — smart deletion based on order frequency analysis

**SOLID Principles applied throughout:**
- **S** — each class has exactly one responsibility (controller routes, service decides, repository queries)
- **O** — new deletion strategies or pricing logic can be added without modifying existing code
- **L** — any repository implementation is substitutable via its interface, including for testing
- **I** — repository and service interfaces are small and entity-specific, never bloated
- **D** — all dependencies injected via interfaces; concrete classes only appear in Program.cs registrations

---

## Project Structure

```
Solution/
│
├── OrderFlow.API/
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   ├── CustomersController.cs
│   │   ├── OrdersController.cs
│   │   └── SellersController.cs          ← new
│   │
│   ├── Services/
│   │   ├── ProductService.cs
│   │   ├── CustomerService.cs
│   │   ├── OrderService.cs
│   │   ├── SellerService.cs              ← new
│   │   └── DataCleanupService.cs         ← new (background job)
│   │
│   ├── Repositories/
│   │   ├── ProductRepository.cs
│   │   ├── CustomerRepository.cs
│   │   ├── OrderRepository.cs
│   │   └── SellerRepository.cs           ← new
│   │
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IProductRepository.cs
│   │   │   ├── ICustomerRepository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   └── ISellerRepository.cs      ← new
│   │   └── Services/
│   │       ├── IProductService.cs
│   │       ├── ICustomerService.cs
│   │       ├── IOrderService.cs
│   │       └── ISellerService.cs         ← new
│   │
│   ├── Models/
│   │   ├── Product.cs                    ← updated (SellerId, OrderCount, DeletionPolicy)
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Seller.cs                     ← new
│   │   └── ArchivedProduct.cs            ← new
│   │
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   ├── CreateProductRequest.cs   ← updated (DeletionPolicy field)
│   │   │   ├── CreateCustomerRequest.cs
│   │   │   ├── CreateOrderRequest.cs
│   │   │   ├── CreateSellerRequest.cs    ← new
│   │   │   └── SellerLoginRequest.cs     ← new
│   │   └── Responses/
│   │       ├── ProductDto.cs             ← updated (SellerName, OrderCount, DeletionPolicy)
│   │       ├── CustomerDto.cs
│   │       ├── OrderDto.cs
│   │       └── SellerDto.cs              ← new
│   │
│   ├── Data/
│   │   └── AppDbContext.cs               ← updated (Sellers, ArchivedProducts, relationships)
│   │
│   ├── Enums/
│   │   ├── OrderStatus.cs
│   │   └── DeletionPolicy.cs             ← new
│   │
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── BadRequestException.cs
│   │   └── ConcurrencyException.cs
│   │
│   ├── Middlewares/
│   │   └── ExceptionMiddleware.cs
│   │
│   ├── Filters/
│   │   └── PreventDuplicateRequestsAttribute.cs
│   │
│   ├── Migrations/                       ← auto-generated by EF Core
│   ├── appsettings.json
│   └── Program.cs
│
└── OrderFlow.ConsoleClient/
    ├── Models/
    │   ├── CustomerDto.cs
    │   ├── ProductDto.cs                 ← updated (SellerName, OrderCount, DeletionPolicy)
    │   ├── OrderDto.cs
    │   ├── OrderItemDto.cs
    │   ├── SellerDto.cs                  ← new
    │   ├── Requests.cs                   ← updated (CreateSellerRequest, SellerLoginRequest)
    │   └── SessionModels.cs              ← updated (SellerSession now has Id and Name)
    │
    ├── Services/
    │   ├── ApiClient.cs                  ← updated (seller endpoints, sellerId on product calls)
    │   ├── CartService.cs
    │   └── AppConfig.cs
    │
    ├── UI/
    │   ├── ConsoleFlow.cs
    │   ├── CustomerFlow.cs
    │   ├── SellerFlow.cs                 ← updated (registry removed, API-based login)
    │   └── ConsoleWriter.cs
    │
    ├── Exceptions/
    │   └── ApiException.cs
    │
    ├── appsettings.json
    └── Program.cs
```

> **Note:** `Services/SellerRegistry.cs` has been removed. Seller identity is now stored in the database and ownership is enforced server-side on every request.

---

## Database Schema

```
Sellers                            Products
─────────────────────              ──────────────────────────────────
Id           PK                    Id             PK
Name                               Name
Email        UNIQUE                Description
Phone                              Price          decimal(18,2)
IsActive                           StockQuantity
CreatedAt                          SellerId       FK → Sellers (nullable)
     │                             OrderCount     ← incremented on every order
     │ 1:N                         LastOrderedAt  ← updated on every order
     ▼                             DeletionPolicy ← AutoPurge / SoftOnly / Manual
Products                           IsDeleted      soft delete
                                   DeletedAt
                                   DeletedBy
                                   CreatedAt
                                   UpdatedAt
                                   RowVersion     concurrency token
                                          │
                                          │ 1:N
                                          ▼
Customers          Orders          OrderItems
──────────────     ──────────────  ───────────────────────────
Id         PK      Id       PK     Id          PK
Name               CustomerId FK   OrderId     FK → Orders
Email      UNIQUE  Status (enum)   ProductId   FK → Products
Phone              TotalAmount     Quantity
Address            ShippingAddress UnitPrice   decimal(18,2) ← snapshotted
IsDeleted          Notes           Subtotal    decimal(18,2) ← stored
DeletedAt          IsDeleted
DeletedBy          DeletedAt
CreatedAt          DeletedBy
     │             CreatedAt
     │ 1:N         UpdatedAt
     ▼
  Orders


ArchivedProducts
─────────────────────────────────
Id                  PK
OriginalProductId   ← original Products.Id
Name
Description
Price               decimal(18,2)
StockQuantityAtDeletion
TotalOrderCount     ← lifetime order count at time of archive
LastOrderedAt
SoftDeletedAt       ← when product was first soft-deleted
ArchivedAt          ← when DataCleanupService moved it here
```

**Delete Cascade Rules:**
- Customer deleted → Orders blocked (Restrict) — cannot delete customer with orders
- Order deleted → OrderItems auto-deleted (Cascade)
- Product deleted → OrderItems blocked (Restrict) — preserves order history
- Seller deleted → Products blocked (Restrict) — cannot delete seller who owns products

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [MySQL 8](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/)

### 1. Clone the repository

```bash
git clone https://github.com/monark1/E-commerceOrderManagementSystem.git
cd E-commerceOrderManagementSystem
```

### 2. Configure the database connection

Open `OrderFlow.API/appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=OrderFlowDb;User=root;Password=YOUR_PASSWORD;"
  }
}
```

### 3. Apply migrations

Open **Package Manager Console** in Visual Studio, select `OrderFlow.API` as the default project:

```
Update-Database
```

This creates the `OrderFlowDb` database with all tables including the new `sellers` and `archived_products` tables.

### 4. Run the API

Press **F5** or run:

```bash
dotnet run --project OrderFlow.API
```

Swagger UI opens at the root URL (e.g. `https://localhost:7001`).

### 5. Configure and run the Console Client

Open `OrderFlow.ConsoleClient/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7001/"
}
```

To run both projects simultaneously in Visual Studio:
- Right-click Solution → Properties → Startup Project
- Select **Multiple startup projects**
- Set both `OrderFlow.API` and `OrderFlow.ConsoleClient` to **Start**
- Press **F5**

---

## API Endpoints

### Products

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | All products visible to customers (soft-deleted excluded) |
| GET | `/api/products/{id}` | Single product by ID |
| GET | `/api/products/low-stock?threshold=5` | Products below stock threshold |
| POST | `/api/products?sellerId={id}` | Create product (ownership assigned in DB) |
| PUT | `/api/products/{id}?sellerId={id}` | Update product (server checks ownership) |
| DELETE | `/api/products/{id}?sellerId={id}` | Delete product (smart deletion applies) |

### Sellers

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/sellers/register` | Register a new seller account |
| POST | `/api/sellers/login` | Login by email — returns seller Id |
| GET | `/api/sellers/{id}` | Get seller details |
| GET | `/api/sellers/{id}/products` | Get ONLY this seller's products |

### Customers

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/customers` | All customers |
| GET | `/api/customers/{id}` | Single customer |
| GET | `/api/customers/{id}/orders` | All orders for a customer (My Orders) |
| POST | `/api/customers` | Register new customer |
| PUT | `/api/customers/{id}` | Update customer |
| DELETE | `/api/customers/{id}` | Soft delete customer |

### Orders

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/orders` | All orders (seller dashboard) |
| GET | `/api/orders/{id}` | Single order with full item details |
| POST | `/api/orders` | Place new order |
| PATCH | `/api/orders/{id}/status` | Update order status (forward only) |
| PATCH | `/api/orders/{id}/cancel` | Cancel order and restore stock |
| DELETE | `/api/orders/{id}` | Soft delete order |

### Order Status Flow

```
Pending (0) → Confirmed (1) → Shipped (2) → Delivered (3)
     └────────────────────────────────────→ Cancelled (4)
```

Status can only move **forward**. Only `Pending` and `Confirmed` orders can be cancelled. Cancellation restores stock for every item in the order.

### Error Response Format

All errors return a consistent JSON shape:

```json
{
  "status": 404,
  "error": "Product with Id 5 not found",
  "detail": null,
  "timestamp": "2024-03-10T14:30:00Z"
}
```

---

## Key Features

### Seller Ownership (Server-Side Enforcement)

Sellers register and login through the API. Their database Id is stored in the `SellerId` column on the `Product` table. When a seller tries to update or delete a product, the API checks `product.SellerId == sellerId` before allowing the operation. This check happens inside `ProductService` — it cannot be bypassed through Swagger, Postman, or any other client.

```
POST /api/sellers/register   → seller stored in MySQL with unique Id
POST /api/sellers/login      → returns seller Id
POST /api/products?sellerId=3  → product.SellerId = 3 saved in DB
PUT  /api/products/5?sellerId=4  → rejected: "You do not own this product"
```

### Smart Deletion — DeletionPolicy

When a seller creates a product, they choose a deletion policy:

| Policy | Value | Behaviour when deleted |
|---|---|---|
| AutoPurge | 0 | System decides based on order frequency (default) |
| SoftOnly | 1 | Always soft delete, data preserved permanently |


**AutoPurge logic at deletion time:**

```
OrderCount ÷ Years active = orders per year

≥ 50 orders/year  →  Soft delete  →  DataCleanupService handles after 1 year
< 50 orders/year  →  Archive immediately  →  Hard delete  →  Archive removed after 2 months
```

### Order Count Tracking

Every time a product is included in an order, `Product.OrderCount` is incremented by the quantity ordered and `Product.LastOrderedAt` is updated. This tracks lifetime demand and is used by the AutoPurge deletion decision.

### Data Lifecycle — DataCleanupService

A background service (`IHostedService`) runs automatically once every **30 days**. It performs two jobs:

**Job 1 — Archive and hard-delete soft-deleted products older than 1 year:**
- Finds products where `IsDeleted = true` AND `DeletedAt < 1 year ago`
- Policy must be `AutoPurge` (SoftOnly and Manual are skipped)
- Copies product data to the `ArchivedProducts` table (snapshot for audit)
- Hard deletes the row from `Products`

**Job 2 — Remove archived products older than 2 months:**
- Finds rows in `ArchivedProducts` where `ArchivedAt < 2 months ago`
- Permanently removes them — data is completely gone

```
Full lifecycle for AutoPurge high-frequency product:

Seller deletes → Soft delete
                     ↓ 1 year later (DataCleanupService Job 1)
             Archive to ArchivedProducts + Hard delete from Products
                     ↓ 2 months later (DataCleanupService Job 2)
             Archived row removed — data completely gone

Full lifecycle for AutoPurge low-frequency product:

Seller deletes → Archive immediately + Hard delete from Products
                     ↓ 2 months later (DataCleanupService Job 2)
             Archived row removed — data completely gone
```

### Soft Delete with Global Query Filter

Products, customers, and orders are never physically deleted when soft-deleted. `AppDbContext` has a Global Query Filter that automatically appends `WHERE IsDeleted = 0` to every query on these tables. No developer can accidentally forget this filter — it is invisible but always active.

```csharp
// Configured once in AppDbContext — active on every single query
modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
```

Recovery is a single field update: `IsDeleted = false`.

### Price Snapshotting

When an order is placed, `Product.Price` is copied into `OrderItem.UnitPrice`. This value never changes. If the product price is updated months later, all historical orders still show what the customer actually paid.

### Optimistic Concurrency

The `Product` entity has a `RowVersion` column (MySQL `TIMESTAMP`) that auto-updates on every row modification. When two requests try to deduct stock simultaneously:

1. Both read stock = 1 and both pass validation
2. First request saves — RowVersion updates from X to Y
3. Second request's `SaveChangesAsync` detects RowVersion mismatch → `DbUpdateConcurrencyException`
4. `OrderService` catches it, clears stale EF Core cache via `DetachAllAsync`, retries
5. On retry: stock = 0, returns `400 Insufficient Stock`
6. If all 3 retries fail: returns `409 Conflict`

### Idempotency Filter

`POST` endpoints for Products, Customers, and Orders are decorated with `[PreventDuplicateRequests]`. The filter computes a SHA256 hash of the request path and body, stores it in `IMemoryCache`, and rejects identical follow-up requests with `409 Conflict` — preventing double-click order placement.

### Global Exception Handling

`ExceptionMiddleware` wraps the entire pipeline. Every unhandled exception is caught here and formatted as a consistent JSON error response. No controller contains a try/catch block.

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 Not Found |
| `BadRequestException` | 400 Bad Request |
| `ConcurrencyException` | 409 Conflict |
| All others | 500 Internal Server Error |

---

## Console Client

A separate `.NET 8` console project that connects to the API over HTTP. All data is real — every action calls a live API endpoint and reads from MySQL.

### Seller Flow

1. **Register** — creates a seller account via `POST /api/sellers/register`
2. **Login** — authenticates via `POST /api/sellers/login`, receives database Id
3. **My Products** — `GET /api/sellers/{id}/products` — shows only this seller's products with order count and deletion policy
4. **Add Product** — `POST /api/products?sellerId={id}` — asks for deletion policy at creation
5. **Update Product** — `PUT /api/products/{id}?sellerId={id}` — API rejects if not the owner
6. **Delete Product** — `DELETE /api/products/{id}?sellerId={id}` — smart deletion applies, API returns message explaining action taken
7. **Low Stock Alert** — `GET /api/products/low-stock?threshold=N`
8. **All Orders** — `GET /api/orders` — full order list with colour-coded status
9. **Update Order Status** — `PATCH /api/orders/{id}/status` — forward only

### Customer Flow

1. **Register** — `POST /api/customers` — name, email, phone, address
2. **Login** — email lookup via `GET /api/customers`
3. **Browse Products** — `GET /api/products` — colour-coded stock status
4. **Cart** — in-memory only, adding the same product twice merges quantity
5. **Checkout** — `POST /api/orders` — cart becomes a real order, stock deducted
6. **My Orders** — `GET /api/customers/{id}/orders` — full history with status colours
7. **Order Detail** — shows all items, prices, subtotals
8. **Cancel Order** — `PATCH /api/orders/{id}/cancel` — only Pending or Confirmed, restores stock

---

## Author

**Monark Bhardwaj**
SDE Intern — Software Development
Windmöller & Hölscher India Pvt. Ltd.
