// Middlewares/ExceptionMiddleware.cs
// Global exception handler — sits in the request pipeline.
// Every unhandled exception flows here and gets converted to a clean JSON response.
// Controllers never need try-catch — this handles everything centrally.

using OrderFlow.API.Exceptions;

namespace OrderFlow.API.Middlewares
{
    public class ExceptionMiddleware
    {
        // _next = the next middleware/controller in the pipeline
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // InvokeAsync is called for every HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass request to next middleware / controller
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                // Resource not found — 404
                await WriteErrorResponse(context, 404, ex.Message);
            }
            catch (BadRequestException ex)
            {
                // Invalid input or business rule violation — 400
                await WriteErrorResponse(context, 400, ex.Message);
            }
            catch (ConcurrencyException ex)
            {
                // Stock conflict — 409
                await WriteErrorResponse(context, 409, ex.Message);
            }
            catch (Exception ex)
            {
                // Anything unexpected — 500
                await WriteErrorResponse(context, 500,
                    "An unexpected error occurred", ex.Message);
            }
        }

        // Writes a consistent JSON error response
        private static async Task WriteErrorResponse(
            HttpContext context,
            int statusCode,
            string message,
            string? detail = null)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Anonymous object → JSON body
            var response = new
            {
                status = statusCode,
                error = message,
                detail = detail,               // null for non-500 errors
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}