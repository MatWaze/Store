using Microsoft.AspNetCore.Identity;
using Store.Models.ViewModels;

namespace Store.Models
{
    public class ProductTarget
    {
        public IEnumerable<Product> Products { get; set; } =
            Enumerable.Empty<Product>();

        public PagingInfo PagingInfo { get; set; } = new();
        public IdentityUser CurrentUser { get; set; } = new();
    }
}
