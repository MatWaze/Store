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

		public required string Name { get; set; }

        public string? NameRu { get; set; }

		public decimal Price { get; set; }

        public decimal ShippingPrice { get; set; }
		
		public int Quantity { get; set; }

		public string Description { get; set; } = "";

		public string? ItemCountry { get; set; }

		public long CategoryId { get; set; }
		public Category? Category { get; set; }

		[NotMapped]
		public IFormFile? ImageFile { get; set; }

		public string? ImageLink { get; set; }

		public string? ImageUrls { get; set; }

		public string? UserId { get; set; }

		public bool Deleted { get; set; } = false;
	}
}
