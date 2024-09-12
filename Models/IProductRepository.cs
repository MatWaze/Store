namespace Store.Models
{
	public interface IProductRepository
	{
		IQueryable<Product> Products { get; }
		IQueryable<Category> Categories { get; }
		
		void SaveProduct(Product product);
		void AddProduct(Product product);
		void DeleteProduct(Product product);
    }
}
