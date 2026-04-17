// Enums/OrderStatus.cs
// This enum represents all possible states an order can be in.
// Stored as int in the database (0, 1, 2...) but used as named values in code.
// The order of values matters — an order moves forward, never backward
// (except to Cancelled).

namespace OrderFlow.API.Enums
{
    public enum OrderStatus
    {
        Pending = 0,   // order placed, not yet confirmed
        Confirmed = 1,   // seller confirmed the order
        Shipped = 2,   // order dispatched
        Delivered = 3,   // customer received the order
        Cancelled = 4    // order cancelled — stock gets restored
    }
}