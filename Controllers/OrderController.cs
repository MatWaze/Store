using System.Globalization;
using System.Text;
using Braintree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;
using Yandex.Checkout.V3;
using System.Web;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace Store.Controllers;

public class OrderController : Controller
{
    public IBraintreeService brainService;
    public IOrderRepository repo;
    public IProductRepository prodRepo;
    public Cart cart;
    private UserManager<IdentityUser> userManager;
    private Client client;
    
    public OrderController(IBraintreeService braintreeService,
        IOrderRepository repository, 
        IProductRepository productRepository,
        Cart cartService,
        UserManager<IdentityUser> usrMgr)
    {
        brainService = braintreeService;
        repo = repository;
        prodRepo = productRepository;
        cart = cartService;
        userManager = usrMgr;
        client = new Client();
    }

    public IActionResult New()
    {
        return View(new Order
        {
            Lines = cart.Lines.ToArray(),
        });
    }

    public IActionResult YooKassaPayment(int orderId)
    {
        ViewBag.ConfirmationToken = YooToken(orderId);
        Console.WriteLine("tok:" + ViewBag.ConfirmationToken);
        return View("YooKassaCheckout", orderId);
    }

    public IActionResult BraintreePayment(int orderId)
    {
        Console.WriteLine("id: " + orderId);
        Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);

        if (order == null)
        {
            return NotFound();
        }
        IBraintreeGateway gateway = brainService.GetGateway();
        string clientToken = gateway.ClientToken.Generate();
        ViewBag.ClientToken = clientToken;

        // Pass amount to view if needed
        Console.WriteLine("name: id: " + order.Name + order.OrderID);
        return View("BraintreeCheckout", order);
    }


    [HttpPost]
    public IActionResult ProcessBraintree([FromForm] int orderId, 
        [FromForm] string nonce)
    {
        Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
        IBraintreeGateway gateway = brainService.GetGateway();
        var request = new TransactionRequest
        {
            Amount = cart.Lines.Sum(e => e.Product.Price * e.Quantity),
            PaymentMethodNonce = nonce,
            Options = new TransactionOptionsRequest
            {
                SubmitForSettlement = true
            }
        };
        Result<Transaction> result = gateway.Transaction
            .Sale(request);
        order.PaymentId = request.OrderId;
        repo.SaveOrder(order);
        if (result.IsSuccess())
        {
            order.PaymentStatus = "Paid";

            Console.WriteLine("Success");
            //var productIdsInOrder = order.Lines.Select(cl => cl.Product.ProductId).ToHashSet();
            //foreach (var prod in prodRepo.Products)
            //{
            //    if (productIdsInOrder.Contains(prod.ProductId)
            //        && prod.Quantity == 0)
            //    {
            //        prod.Deleted = true;
            //        prodRepo.SaveProduct(prod);
            //    }
            //}
            //repo.SaveOrder(order);
            cart.Clear(Request);
            repo.SaveOrder(order);

            return RedirectToPage("/Completed",
                new { orderId = order.OrderID });
        }
        else
        {
            var productIdsInOrder = cart.Lines.Select(cl => cl.Product.ProductId).ToHashSet();
            foreach (var prod in prodRepo.Products)
            {
                if (productIdsInOrder.Contains(prod.ProductId))
                {
                    CartLine? cartLine = order.Lines
                        .FirstOrDefault(p => p.Product.ProductId == prod.ProductId);
                    prod.Quantity += cartLine.Quantity;
                    prodRepo.SaveProduct(prod);
                }
            }
            //repo.DeleteOrder(order);
            return RedirectToAction("Products", "Home");
        }
    }

    private NewReceipt CreateYooReceipt()
    {
        List<ReceiptItem> receiptItems = new();
        foreach (var item in cart.Lines)
        {
            ReceiptItem receiptItem = new ReceiptItem
            {
                Amount = new Amount { Value = Math.Round(item.Quantity * item.Product.Price, 2), Currency = "RUB" },
                Description = item.Product.Description,
                Quantity = item.Quantity,
                VatCode = VatCode.Vat0,
                PaymentMode = PaymentMode.FullPayment,
                PaymentSubject = PaymentSubject.Commodity,
            };
            receiptItems.Add(receiptItem);
        }
        NewReceipt newReceipt = new NewReceipt
        {
            Customer = new Yandex.Checkout.V3.Customer
            {
                FullName = "Матевос Амазарян Агванович",
                Email = "coolmartun@gmail.com"
            },
            Items = receiptItems,
            Send = true,
        };
        return newReceipt;
    }

    public async Task<IActionResult> Callback2(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            Console.WriteLine("error");
            return RedirectToAction("Error");
        }

        string url = "https://yookassa.ru/oauth/v2/token";
        string clientId = "uhedu6u4q1b6mp2tvimjads6bng8s72q";
        string clientSecret = "Fqd60rf0uCGZxGTPnPX6QAFx1YN6BUeOFTTYnfLKYIvOyOx1v2T0PnGvXwWtG3pm";
        using (var client = new HttpClient())
        {
            var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var requestData = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code }
                };
            var content = new FormUrlEncodedContent(requestData);

            HttpResponseMessage response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseData = JObject.Parse(responseString);

                string accessToken = responseData["access_token"].ToString();
                Console.WriteLine($"Access Token: {accessToken}");

                return RedirectToAction("Index", "Home"); // Redirect to a secure page or home page
            }
            else
            {
                // Handle error response
                string errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorResponse}");
                return RedirectToAction("Error"); // Redirect to an error page
            }
        }
    }

    public IActionResult Error()
    {
        return RedirectToAction("Index", "Home");
    }

    private string YooToken(int orderId)
    {
        Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
        decimal amount = Math.Round(cart.Lines.Sum(e => e.Product.Price * e.Quantity), 2);
        var newPayment = new NewPayment
        {
            Amount = new Amount { Value = amount, Currency = "RUB" },
            Confirmation = new Confirmation { Type = ConfirmationType.Embedded },
            Receipt = CreateYooReceipt()
        };

        Payment payment = client.CreatePayment(newPayment);
        //var user = await userManager.GetUserAsync(User);
        order.PaymentId = payment.Id;

        repo.SaveOrder(order);
        var confirmationToken = payment.Confirmation.ConfirmationToken;
        return confirmationToken;
    }

    public async Task<IActionResult> Note()
    {
        AsyncClient asyncClient = client.MakeAsync();
        // Enable buffering to allow multiple reads of the request body
        Request.EnableBuffering();

        // Read the request body
        string body;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            // Reset the stream position to allow further reading if necessary
            Request.Body.Position = 0;
        }
        var notification = Client.ParseMessage(Request.Method, Request.ContentType, body);

        if (notification is PaymentWaitingForCaptureNotification paymentWaitingForCaptureNotification)
        {
            Payment payment = paymentWaitingForCaptureNotification.Object;

            if (payment.Paid)
            {
                Console.WriteLine($"Got message: payment.id={payment.Id}, payment.paid={payment.Paid}");

                Payment p = await asyncClient.CapturePaymentAsync(payment.Id);
                string? receiptId = null;

                await foreach (var receipt in asyncClient.GetReceiptsAsync())
                {
                    if (receipt.PaymentId == p.Id)
                    {
                        receiptId = receipt.Id;
                        break;
                    }
                }

                Receipt? r = await asyncClient.GetReceiptAsync(receiptId);
                if (r != null)
                {
                    Console.WriteLine("Receipt found: " + r.ToJson());
                }
                else
                {
                    Console.WriteLine("Receipt not found");
                }
                return Ok();
            }
        }

        return BadRequest();
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
                order.PaymentStatus = "Pending";

                repo.SaveOrder(order);

                if (order.PaymentMethod == "Braintree")
                {
                    return RedirectToAction("BraintreePayment", new { orderId = order.OrderID } );
                }
                else/* if (order.PaymentMethod == "YooKassa")*/
                {
                    return RedirectToAction("YooKassaPayment", new { orderId = order.OrderID });
                }
            }
            else
            {
                Console.WriteLine("Empty cart");
                var productIdsInOrder = cart.Lines.Select(cl => cl.Product.ProductId).ToHashSet();

                foreach (var prod in prodRepo.Products)
                {
                    if (productIdsInOrder.Contains(prod.ProductId))
                    {
                        CartLine? cartLine = order.Lines
                            .FirstOrDefault(p => p.Product.ProductId == prod.ProductId);
                        prod.Quantity += cartLine.Quantity;
                        prodRepo.SaveProduct(prod);
                    }
                }
                return RedirectToAction("Products", "Home");
            }
        }
        else
        {
            //var productIdsInOrder = cart.Lines.Select(cl => cl.Product.ProductId).ToHashSet();
            //foreach (var prod in prodRepo.Products)
            //{
            //    if (productIdsInOrder.Contains(prod.ProductId))
            //    {
            //        CartLine? cartLine = order.Lines
            //            .FirstOrDefault(p => p.Product.ProductId == prod.ProductId);
            //        if (cartLine != null)
            //        {
            //            prod.Quantity += cartLine.Quantity;
            //            prodRepo.SaveProduct(prod);
            //        }
            //    }
            //}
            return RedirectToAction("Checkout");
        }
    }
}