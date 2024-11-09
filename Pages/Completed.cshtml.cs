using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Infrastructure;
using Store.Models;

namespace Store.Pages;

public class Completed : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? OrderId { get; set; }

    private readonly IOrderRepository orderRepo;
    private readonly IProductRepository prodRepo;
    private readonly Models.Cart cart;
    private IRazorViewToStringRenderer razorView;
    private ISendEmail sendEmail;
    private UserManager<ApplicationUser> userManager;

    public Completed(
        IOrderRepository ordRepo,
        IProductRepository productRepo,
        Models.Cart cartObj,
        IRazorViewToStringRenderer razor,
        ISendEmail send,
        UserManager<ApplicationUser> usrMgr)
    {
        orderRepo = ordRepo;
        prodRepo = productRepo;
        cart = cartObj;
        razorView = razor;
        sendEmail = send;
        userManager = usrMgr;
    }

    public async void OnGet()
    {
        Order? ord = orderRepo
            .Orders
            .FirstOrDefault(o => o.OrderID == int.Parse(OrderId));

        ord.PaymentStatus = "Paid";

        List<Product> productIdsInOrder = ord.Lines.Select(cl => cl.Product).ToList();
        foreach (Product prod in productIdsInOrder)
        {
            if (prod.Quantity > 0)
            {
                CartLine? line = ord.Lines
                    .ToList()
                    .Find(cl => cl.Product.ProductId == prod.ProductId);
                prod.Quantity -= line.Quantity;
            }
            else if (prod.Quantity == 0)
            {
                prod.Deleted = true;
            }
            prodRepo.SaveProduct(prod);
        }
        orderRepo.SaveOrder(ord);

        string htmlContent = await razorView
           .RenderViewToStringAsync<Order>("EmailOrderNotification", ord);

        var user = await userManager.GetUserAsync(User);
        await sendEmail.SendEmailAsync(user!.Email!, "ILoveParts order created", htmlContent);

        cart.Clear();
    }
}