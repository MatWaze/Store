using System.Text.Json.Serialization;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Store.Infrastructure;

namespace Store.Models
{
    public class SessionCart : Cart
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public ISession? Session { get; set; }
        public string? userName { get; set; }
        
        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session =
                services.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Session;
            string? userName = services.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.User?.Identity?.Name;

            var cart = session?.GetJson<SessionCart>($"cart_{userName}") 
                ?? new SessionCart();
            cart.Session = session;
            cart.userName = userName;
            return cart;
        }

        public override void AddItem(Product product, int quantity)
        {
            base.AddItem(product, quantity);
            Session?
                .SetJson($"cart_{userName}", this);
        }

        public override void RemoveLine(Product product)
        {
            base.RemoveLine(product);
            Session?
                .SetJson($"cart_{userName}", this);

        }

        public override void Clear()
        {
            base.Clear();
            Session?
                .Remove($"cart_{userName}");
        }

    }
}