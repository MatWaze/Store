using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Store.Infrastructure;
using System.Linq;

namespace Store.Models
{
    public class EFProductRepository : IProductRepository
    {
        private DataContext context;
        private UserManager<ApplicationUser> userManager;
        public EFProductRepository(DataContext dataContext, UserManager<ApplicationUser> usrMgr)
        {
            context = dataContext;
            userManager = usrMgr;
            //context.Products.Load();  // Preload all products
            //context.Categories.Load();
        }

        public IQueryable<Product> Products => context.Products;

        public IQueryable<Category> Categories => context.Categories;
        public void SaveProduct(Product product)
        {
            context.SaveChanges();
        }

        public void AddProduct(Product product)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }

        public void DeleteProduct(Product product)
        {
            context.Remove(product);
            context.SaveChanges();
        }
    }
}
