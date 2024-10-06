using Microsoft.AspNetCore.Identity;
using Store.Models.ViewModels;

namespace Store.Models
{
    public class ProductTarget<T>
    {
        public IEnumerable<T> Products { get; set; } =
            Enumerable.Empty<T>();

        public PagingInfo PagingInfo { get; set; } = new();
        public ApplicationUser CurrentUser { get; set; } = new();
    }
}
