using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models;

namespace Store.EFCoreTypes
{
    public class UserEntity : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> modelBuilder)
        {
            modelBuilder
                .HasOne(u => u.Address)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.AddressId);
        }
    }
}
