using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models;

namespace Store.EFCoreTypes
{
	public class ProductEntity : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> modelBuilder)
		{
			modelBuilder
				.Property(p => p.Name)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(1000);
			
			modelBuilder
				.Property(p => p.NameRu)
				.HasColumnType("character varying")
				.HasMaxLength(1000);
			
			modelBuilder
				.Property(p => p.ItemCountry)
				.HasColumnType("character varying")
				.HasMaxLength(2);
			
			modelBuilder
				.Property(p => p.Description)
				.HasColumnType("character varying")
				.HasMaxLength(500);

			modelBuilder
				.Property(p => p.ImageLink)
				.HasColumnType("character varying")
				.HasMaxLength(1000);

			modelBuilder
				.Property(p => p.ImageUrls)
				.HasColumnType("character varying")
				.HasMaxLength(3000);

			modelBuilder
				.Property(p => p.Price)
				.HasColumnType("DECIMAL(8, 2)");

			modelBuilder
				.Property(p => p.ShippingPrice)
				.HasColumnType("DECIMAL(8, 2)");

			modelBuilder
				.Property(p => p.Quantity)
				.HasColumnType("DECIMAL(8, 2)");

			modelBuilder
				.Property(p => p.ProductId)
				.IsRequired()
				.UseIdentityColumn();
		}
	}
}
