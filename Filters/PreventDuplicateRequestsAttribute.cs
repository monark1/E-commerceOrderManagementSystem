using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OrderFlow.API.Filters
{
    // Inherits from ActionFilterAttribute so it can be used as a [Tag] on controllers
    public class PreventDuplicateRequestsAttribute : ActionFilterAttribute
    {
        private readonly int _lockTimeSeconds;

        // Default lockout is 10 seconds
        public PreventDuplicateRequestsAttribute(int lockTimeSeconds = 10)
        {
            _lockTimeSeconds = lockTimeSeconds;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Resolve the memory cache from the dependency injection container
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            // 2. Build a unique string using the API route (e.g., /api/orders)
            var requestPath = context.HttpContext.Request.Path.ToString();

            // 3. Serialize the incoming DTO (the request body) into a JSON string
            var payloadJson = JsonSerializer.Serialize(context.ActionArguments);

            // 4. Hash the JSON string to create a compact, secure cache key
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
            var payloadHash = Convert.ToBase64String(hashBytes);

            // Combine path and hash
            var cacheKey = $"Idempotency_{requestPath}_{payloadHash}";

            // 5. Check if this exact payload hit this exact endpoint recently
            if (cache.TryGetValue(cacheKey, out _))
            {
                // Reject the duplicate request immediately — skips the controller entirely
                context.Result = new ConflictObjectResult(new
                {
                    error = "Duplicate request detected. Please wait a moment before submitting again."
                });
                return;
            }

            // 6. Lock this payload in the cache for the specified time
            cache.Set(cacheKey, true, TimeSpan.FromSeconds(_lockTimeSeconds));

            // 7. Proceed to the Controller
            await next();
        }
    }
}