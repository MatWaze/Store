using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Mvc.ControllerBase;
namespace Store.Models
{
    public class EFProductRepository : IProductRepository
    {
        private DataContext context;
        private UserManager<IdentityUser> userManager;
        public EFProductRepository(DataContext dataContext, UserManager<IdentityUser> usrMgr)
        {
            context = dataContext;
            userManager = usrMgr;
        }

        public IQueryable<Product> Products => context.Products;
        // public IQueryable<IdentityUser> Users => context.Users;
        public IQueryable<Category> Categories => context.Categories;
        public void SaveProduct(Product product)
        {
            context.SaveChanges();
        }

        public async void AddProduct(Product product)
        {
            var category = context.Categories.FirstOrDefault(c => c.CategoryId == product.CategoryId);
            if (category != null)
            {
                product.Category = category;
                string existingUser =  (await userManager.FindByIdAsync(product.UserId)).Id;
                if (existingUser != null)
                {
                    product.UserId = existingUser;
                }
                
                context.Products.Add(product);
                context.SaveChanges();
            }
            else
            {
                throw new Exception("Invalid CategoryId");
            }
        }

        public void DeleteProduct(Product product)
        {
            context.Remove(product);
            context.SaveChanges();
        }
    }
}