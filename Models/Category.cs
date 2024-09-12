namespace Store.Models
{
	public class Category
	{
		public long CategoryId { get; set; }

		public long EbayCategoryId { get; set; }
		public required string Name { get; set; }

		public IEnumerable<Product>? Products { get; set; }
	}
}
