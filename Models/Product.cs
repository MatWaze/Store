using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections.Immutable;

namespace Store.Models
{
	public class Product
	{
		public long ProductId { get; set; }

		public string EbayProductId { get; set; }

		[Remote("Name", "Validation", ErrorMessage = "Please enter a name")]
		public required string Name { get; set; }

        public string? NameRu { get; set; }

		[Range(1, 999999, ErrorMessage = "Please enter a positive price")]
		[Column(TypeName = "decimal(8, 2)")]
		public decimal Price { get; set; }

        [Range(1, 999999, ErrorMessage = "Please enter a positive price")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal ShippingPrice { get; set; }
		
		[Range(1, 99999, ErrorMessage = "Please enter a positive quantity")]
		public int Quantity { get; set; }

		[MaxLength(1000)]
		public string Description { get; set; } = "";

		public string ItemCountry { get; set; }

		public long CategoryId { get; set; }
		public Category? Category { get; set; }

		[NotMapped]
		public IFormFile? ImageFile { get; set; }
		public string? ImageLink { get; set; }

		public string? ImageUrls { get; set; }

		public string? UserId { get; set; }
		// public IdentityUser? User { get; set; }

		public bool Deleted { get; set; } = false;
	}
}
