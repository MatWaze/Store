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
        private readonly IBraintreeGateway brainGateway;
        private readonly IOrderRepository repo;
        private readonly IProductRepository prodRepo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly Cart cart;
        private readonly ILogger<BraintreeController> logger;

        public BraintreeController(IBraintreeGateway brainTree,
            IOrderRepository repoOrder,
            Cart cartService,
            IProductRepository prodRepository,
            UserManager<ApplicationUser> usrMgr,
            ILogger<BraintreeController> log
        )
        {
            brainGateway = brainTree;
            repo = repoOrder;
            cart = cartService;
            prodRepo = prodRepository;
            userManager = usrMgr;
            logger = log;
        }

        public IActionResult BraintreePayment(int orderId)
        {
            Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
            
            if (order == null)
            {
                return NotFound();
            }

            order.PaymentStatus = "Pending";

            repo.SaveOrder(order);
            //IBraintreeGateway gateway = brainService.CreateGateway();
            string clientToken = brainGateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;

            return View("Views/Order/BraintreeCheckout.cshtml", order);
        }

        [HttpPost]
        public IActionResult ProcessBraintree([FromForm] int orderId,
            [FromForm] string nonce)
        {
            Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
            order.PaymentId = nonce;

            //IBraintreeGateway gateway = brainService.CreateGateway();
            var request = new TransactionRequest
            {
                Amount = cart.ComputeTotalValue(),
                PaymentMethodNonce = nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            Result<Transaction> result = brainGateway.Transaction
                .Sale(request);
            repo.SaveOrder(order);
            if (result.IsSuccess())
            {
                order.PaymentId = request.OrderId;
                logger.LogInformation("Order {orderId} for {name} completed",
                    order.OrderID, User.Identity.Name);
                return RedirectToPage("/Completed",
                    new { OrderNonce = nonce });
            }
            else
            {
				logger.LogCritical("Order {orderId} for {name} failed",
					order.OrderID, User.Identity.Name);
				string? errorMessage = result
                    .Errors
                    .All()
                    .FirstOrDefault()?
                    .Message;
                return RedirectToAction("Canceled", "YooKassa", new
                {
                    errorMsg = errorMessage
                });
            }
        }
    }
}
