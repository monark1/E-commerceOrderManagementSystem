// DTOs/Responses/CustomerDto.cs
// What the API returns for customer data.

namespace OrderFlow.API.DTOs.Responses
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}