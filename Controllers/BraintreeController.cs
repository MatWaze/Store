using Braintree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Store.Infrastructure;
using Store.Models;
using System.Security.Cryptography;

namespace Store.Controllers
{
    public class BraintreeController : Controller
    {

        private IBraintreeGateway brainGateway;
        private IOrderRepository repo;
        private IProductRepository prodRepo;
        private UserManager<ApplicationUser> userManager;
        private IRazorViewToStringRenderer razorView;
        private ISendEmail sendEmail;
        public Cart cart;

        public BraintreeController(IBraintreeGateway brainTree,
            IOrderRepository repoOrder,
            Cart cartService,
            IProductRepository prodRepository,
            UserManager<ApplicationUser> usrMgr,
            IRazorViewToStringRenderer razorViewToString,
            ISendEmail send)
        {
            brainGateway = brainTree;
            repo = repoOrder;
            cart = cartService;
            prodRepo = prodRepository;
            userManager = usrMgr;
            razorView = razorViewToString;
            sendEmail = send;
        }

        public IActionResult BraintreePayment(int orderId)
        {
            Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }
            //IBraintreeGateway gateway = brainService.CreateGateway();
            string clientToken = brainGateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;

            Console.WriteLine("name: id: " + order.Name + order.OrderID);
            return View("Views/Order/BraintreeCheckout.cshtml", order);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessBraintree([FromForm] int orderId,
            [FromForm] string nonce)
        {
            Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
            //IBraintreeGateway gateway = brainService.CreateGateway();
            var request = new TransactionRequest
            {
                Amount = cart.Lines.Sum(e => e.Product.Price * e.Quantity),
                PaymentMethodNonce = nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            Result<Transaction> result = brainGateway.Transaction
                .Sale(request);
            order.PaymentId = request.OrderId;
            repo.SaveOrder(order);
            if (result.IsSuccess())
            {
                order.PaymentStatus = "Paid";
                Product[] productIdsInOrder = order.Lines.Select(cl => cl.Product).ToArray();

                foreach (Product prod in productIdsInOrder)
                {
                    if (prod.Quantity > 0)
                    {
                        CartLine? line = order.Lines
                            .ToList<CartLine>()
                            .Find(cl => cl.Product.ProductId == prod.ProductId);
                        prod.Quantity -= line.Quantity;
                    }
                    if (prod.Quantity == 0)
                    {
                        prod.Deleted = true;
                    }
                    prodRepo.SaveProduct(prod);
                }
                repo.SaveOrder(order);
                ApplicationUser? user = await userManager.GetUserAsync(User);
                string htmlContent = await razorView
                  .RenderViewToStringAsync<Order>("EmailOrderNotification", order);

                await sendEmail.SendEmailAsync(user?.Email, "ILoveParts order created", htmlContent);
                return RedirectToPage("/Completed",
                    new { orderId = order.OrderID });
            }
            return RedirectToAction("Index", "Home"); // on fail
        }
    }
}
