// DTOs/Requests/CreateCustomerRequest.cs
// Data required to register a new customer.

using System.ComponentModel.DataAnnotations;

namespace OrderFlow.API.DTOs.Requests
{
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(400)]
        public string? Address { get; set; }
    }
}