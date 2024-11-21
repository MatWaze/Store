using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.ViewModels;

namespace Store.EFCoreTypes
{
    public class AddressEntity : IEntityTypeConfiguration<AddressViewModel>
    {
        public void Configure(EntityTypeBuilder<AddressViewModel> modelBuilder)
        {
            modelBuilder
                .Property(a => a.Country)
                .HasColumnType("character varying")
                .HasMaxLength(100);

            modelBuilder
                .Property(a => a.City)
                .HasColumnType("character varying")
                .HasMaxLength(100);

            modelBuilder
                .Property(a => a.Street)
                .HasColumnType("character varying")
                .HasMaxLength(1000);

            modelBuilder
                .Property(a => a.Region)
                .HasColumnType("character varying")
                .HasMaxLength(100);

            modelBuilder
                .Property(a => a.PostalCode)
                .HasColumnType("character varying")
                .HasMaxLength(10);

            modelBuilder
                .Property(a => a.Id)
                .IsRequired()
                .UseIdentityColumn();
        }
    }
}
