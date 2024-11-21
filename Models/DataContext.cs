using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Store.EFCoreTypes;

namespace Store.Models
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options)
			: base(options) {}
		public virtual DbSet<Product> Products => Set<Product>();
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<Order> Orders => Set<Order>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			Configuration.ConfigureEntity(modelBuilder.Entity<Product>());
			Configuration.ConfigureEntity(modelBuilder.Entity<Order>());
		}
	}
}
