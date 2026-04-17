// Exceptions/ConcurrencyException.cs
// Thrown when a stock update fails due to a race condition.
// Example: Two users order the last item simultaneously.
// EF Core detects the conflict via RowVersion — we catch it and throw this.
// ExceptionMiddleware catches this and returns HTTP 409 Conflict.

namespace OrderFlow.API.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}