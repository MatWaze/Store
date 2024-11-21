using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
	public class Category
	{
		public long CategoryId { get; set; }

		public long EbayCategoryId { get; set; }

		[MaxLength(100)]
		public required string Name { get; set; }

		public IEnumerable<Product>? Products { get; set; }
	}
}
