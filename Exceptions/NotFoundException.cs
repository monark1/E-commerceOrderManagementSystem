// Exceptions/NotFoundException.cs
// Thrown when a requested resource doesn't exist in the DB.
// Example: GetProduct(id) where id doesn't exist → throw this.
// ExceptionMiddleware catches this and returns HTTP 404.

namespace OrderFlow.API.Exceptions
{
    public class NotFoundException : Exception
    {
        // Calls base Exception constructor with our custom message
        public NotFoundException(string message) : base(message) { }
    }
}