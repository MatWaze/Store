using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Store.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? YooKassaAccessToken { get; set; }
        public string? FullName { get; set; }
        public int AddressId { get; set; }
		public AddressViewModel? Address { get; set; }
    }
}
