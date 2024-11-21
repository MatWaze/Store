using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models;
using Store.EFCoreTypes;
using Store.Models.ViewModels;

namespace Store.EFCoreTypes
{
	public static class Configuration
	{
		public static void ConfigureEntity<T>(EntityTypeBuilder<T> builder) where T : class
		{
			switch (typeof(T))
			{
				case Type type when type == typeof(Order):
					new OrderEntity().Configure((builder as EntityTypeBuilder<Order>)!);
					break;
				case Type type when type == typeof(Product):
					new ProductEntity().Configure((builder as EntityTypeBuilder<Product>)!);
					break;
				case Type type when type == typeof(AddressViewModel):
					new AddressEntity().Configure((builder as EntityTypeBuilder<AddressViewModel>)!);
					break;
				case Type type when type == typeof(ApplicationUser):
					new UserEntity().Configure((builder as EntityTypeBuilder<ApplicationUser>)!);
					break;
				default:
					break;
			}
		}
	}
}
