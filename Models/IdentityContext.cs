using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Store.EFCoreTypes;
using Store.Models.ViewModels;

namespace Store.Models
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> opts)
        : base(opts) { }

        public DbSet<AddressViewModel> Addresses => Set<AddressViewModel>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            Configuration.ConfigureEntity(builder.Entity<AddressViewModel>());
            Configuration.ConfigureEntity(builder.Entity<ApplicationUser>());
        }
    }
}
