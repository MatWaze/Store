using Microsoft.AspNetCore.Identity;

namespace Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? YooKassaAccessToken { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Street { get; set; }
    }
}
