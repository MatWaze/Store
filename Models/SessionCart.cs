using System.Text.Json.Serialization;
using Azure.Core;
using Store.Infrastructure;

namespace Store.Models
{
    public class SessionCart : Cart
    {
        [JsonIgnore]
        public ISession? Session { get; set; }
        
        public static Cart GetCart(IServiceProvider services)
        {
            ISession? session =
                services.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Session;
                    
            var cart = session?.GetJson<SessionCart>
                ($"cart_{services.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Request.Cookies[".AspNetCore.Identity.Application"]}") 
                ?? 
                new SessionCart();
            cart.Session = session;
            return cart;
        }

        public override void AddItem(Product product, int quantity,
            HttpRequest request)
        {
            base.AddItem(product, quantity, request);
            Session?
                .SetJson($"cart_{request
                    .Cookies[".AspNetCore.Identity.Application"]}", this);
        }

        public override void RemoveLine(Product product, HttpRequest request)
        {
            base.RemoveLine(product, request);
            Session?
                .SetJson($"cart_{request
                    .Cookies[".AspNetCore.Identity.Application"]}", this);

        }

        public override void Clear(HttpRequest request)
        {
            base.Clear(request);
            Session?
                .Remove($"cart_{request
                    .Cookies[".AspNetCore.Identity.Application"]}");
        }

    }
}