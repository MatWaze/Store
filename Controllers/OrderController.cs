using Braintree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;

using Microsoft.AspNetCore.Authorization;

namespace Store.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class OrderController : Controller
{
    public IBraintreeGateway brainGateway;
    public IOrderRepository repo;
    public HttpClient httpClient;
    public IProductRepository prodRepo;
    public Cart cart;
    private UserManager<ApplicationUser> userManager;

    public OrderController(IBraintreeGateway braintreeService,
        IOrderRepository repository,
        IProductRepository productRepository,
        Cart cartService,
        UserManager<ApplicationUser> usrMgr,
        HttpClient client,
        )
    {   
        brainGateway = braintreeService;
        repo = repository;
        prodRepo = productRepository;
        cart = cartService;
        userManager = usrMgr;
        httpClient = client;
    }

    public async Task<IActionResult> New()
    {
        ViewBag.User = await userManager.GetUserAsync(User);
        return View(new Order
        {
            Lines = cart.Lines.ToArray(),
        });
    }

    public IActionResult Error()
    {
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order order)
    {
        if (cart.Lines.Count() == 0)
        {
            ModelState.AddModelError("",
                "Sorry, your cart is empty!");
        }
        if (ModelState.IsValid)
        {
            order.Lines = cart.Lines;
            if (order.Lines.Count() > 0)
            {
                var user = await userManager.GetUserAsync(User);
                order.UserId = user.Id;
                order.Lines = cart.Lines;
                order.PaymentStatus = "Initiated";

                repo.SaveOrder(order);

                if (order.PaymentMethod == "Braintree")
                {
                    return RedirectToAction("BraintreePayment", "Braintree", new { orderId = order.OrderID });
                }
                else/* if (order.PaymentMethod == "YooKassa")*/
                {
					return RedirectToAction("Auth2", "YooKassa", new { orderId = order.OrderID });
                }
            }
            else
            {
                Console.WriteLine("Empty cart");
                return RedirectToAction("Products", "Home");
            }
        }
        else
        {
            return RedirectToAction("New");
        }
    }
}
