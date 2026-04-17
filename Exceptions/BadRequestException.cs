// Exceptions/BadRequestException.cs
// Thrown when request data is invalid or a business rule is violated.
// Examples:
//   - Insufficient stock when placing order
//   - Trying to cancel a Delivered order
// ExceptionMiddleware catches this and returns HTTP 400.

namespace OrderFlow.API.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}