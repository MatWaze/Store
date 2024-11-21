using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models;

namespace Store.EFCoreTypes
{
	public class OrderEntity : IEntityTypeConfiguration<Order>
	{
		public void Configure(EntityTypeBuilder<Order> modelBuilder)
		{
			modelBuilder
				.Property(o => o.Name)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(1000);

			modelBuilder
				.Property(o => o.Line1)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(1000);
			
			modelBuilder
				.Property(o => o.Line2)
				.HasColumnType("character varying")
				.HasMaxLength(1000);
			
			modelBuilder
				.Property(o => o.Line3)
				.HasColumnType("character varying")
				.HasMaxLength(1000);

			modelBuilder
				.Property(o => o.State)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(100);

			modelBuilder
				.Property(o => o.City)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(100);

			modelBuilder
				.Property(o => o.Country)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(100);

			modelBuilder
				.Property(o => o.PaymentId)
				.HasColumnType("character varying")
				.HasMaxLength(100);

			modelBuilder
				.Property(o => o.PaymentStatus)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(100);

			modelBuilder
				.Property(o => o.PaymentMethod)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(50);

			modelBuilder
				.Property(o => o.Zip)
				.IsRequired()
				.HasColumnType("character varying")
				.HasMaxLength(10);

		}
	}
}
