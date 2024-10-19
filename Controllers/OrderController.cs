using System.Text;
using Braintree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Infrastructure;
using Store.Models;
using Yandex.Checkout.V3;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
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
    private IConfiguration config;
    private UserManager<ApplicationUser> userManager;
    private IRazorViewToStringRenderer razorView;
    private ISendEmail sendEmail;

    public OrderController(IBraintreeGateway braintreeService,
        IOrderRepository repository,
        IProductRepository productRepository,
        Cart cartService,
        UserManager<ApplicationUser> usrMgr,
        IConfiguration configuration,
        HttpClient client,
        IRazorViewToStringRenderer razorViewToString,
        ISendEmail send
        )
    {   
        brainGateway = braintreeService;
        repo = repository;
        prodRepo = productRepository;
        cart = cartService;
        userManager = usrMgr;
        config = configuration;
        httpClient = client;
        razorView = razorViewToString;
        sendEmail = send;
    }

    public async Task<string?> GetUserYooAccessToken()
    {
        var user = await userManager.GetUserAsync(User);
        string? tok = user?.YooKassaAccessToken;
        return tok;
    }

    public async Task<IActionResult> New()
    {
        ViewBag.User = await userManager.GetUserAsync(User);
        return View(new Order
        {
            Lines = cart.Lines.ToArray(),
        });
    }

    public async Task<IActionResult> YooKassaPayment(int orderId, string accessToken)
    {
        CreateYooWebHook("waiting_for_capture", accessToken);
        ViewBag.ConfirmationToken = await YooToken(orderId, accessToken);
        Console.WriteLine("Confirmation token: " + ViewBag.ConfirmationToken);
        return View("Views/Order/YooKassaCheckout", orderId);
    }

    public async Task<NewReceipt> CreateYooReceipt()
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
        var user = await userManager.GetUserAsync(User);
        NewReceipt newReceipt = new NewReceipt
        {
            Customer = new Yandex.Checkout.V3.Customer
            {
                FullName = user.FullName,
                Email = user.Email
            },
            Items = receiptItems,
            Send = true,
        };
        return newReceipt;
    }

    public async Task<IActionResult> Auth2(int orderId)
    {
        ApplicationUser? user = await userManager.GetUserAsync(User);
        if (user?.YooKassaAccessToken == null /* or expired */)
        {
            string apiUrl = $"https://yookassa.ru/oauth/v2/authorize?response_type=code&client_id={config["YooKassaApi:ClientId"]}&state=test-user";
            return Redirect(apiUrl);
		}
        else
        {
            Console.WriteLine("user already authenticated");
			return RedirectToAction("YooKassaPayment", "Braintree",
                new { orderId = orderId, accessToken = user.YooKassaAccessToken });
		}
	}

    public async Task<IActionResult> Callback2(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            Console.WriteLine("error");
            return RedirectToAction("Index", "Home");
        }
        string url = "https://yookassa.ru/oauth/v2/token";
       
        var byteArray = Encoding
            .ASCII.GetBytes($"{config["YooKassaApi:ClientId"]}:{config["YooKassaApi:ClientSecret"]}");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var requestData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code }
        };
        var content = new FormUrlEncodedContent(requestData);
        HttpResponseMessage response = await httpClient.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JObject.Parse(responseString);
                
            ApplicationUser? user = await userManager.GetUserAsync(User);
            user.YooKassaAccessToken = responseData["access_token"]?.ToString();
            await userManager.UpdateAsync(user);

            Order? ord = repo.Orders.FirstOrDefault(o => o.UserId == user.Id);
			return RedirectToAction("YooKassaPayment", "Braintree",
                new { orderId = ord?.OrderID, accessToken = user.YooKassaAccessToken });
        }
        else
        {
            string errorResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {errorResponse}");
            return RedirectToAction("Index", "Home");
        }
    }

    public IActionResult Error()
    {
        return RedirectToAction("Index", "Home");
    }

    private async Task<string> YooToken(int orderId, string accessToken)
    {
        Order? order = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
        decimal amount = Math.Round(cart.Lines.Sum(e => e.Product.Price * e.Quantity), 2);
        var newPayment = new NewPayment
        {
            Amount = new Amount { Value = amount, Currency = "RUB" },
            Confirmation = new Confirmation { Type = ConfirmationType.Embedded },
            Receipt = await CreateYooReceipt(),
            Metadata = new Dictionary<string, string> 
            { 
                { "OrderID", orderId.ToString() },
                { "AccessToken", accessToken }
            },
        };
        Console.WriteLine("access token: " + accessToken);
        Client yooClient = new Client(accessToken: accessToken);
        Payment payment = yooClient.CreatePayment(newPayment);
        //var user = await userManager.GetUserAsync(User);
        order.PaymentId = payment.Id;
        Console.WriteLine($"Conf token: {payment.Confirmation.ConfirmationToken}");
        repo.SaveOrder(order);
        var confirmationToken = payment.Confirmation.ConfirmationToken;
        return confirmationToken;
    }

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("Braintree/Notification")]
    public async Task<IActionResult> Notification()
    {
        // Enable buffering to allow multiple reads of the request body
        Request.EnableBuffering();

        // Read the request body
        string body;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            // Reset the stream position to allow further reading if necessary
            Request.Body.Position = 0;
            reader.Close();
        }
        var notification = Client.ParseMessage(Request.Method, Request.ContentType, body);
        if (notification is PaymentWaitingForCaptureNotification captureNote)
        {
            Payment payment = captureNote.Object;
            Console.WriteLine("Waiting for capture");
            if (payment.Paid)
            {
                Client yooClient = new Client(accessToken: payment.Metadata["AccessToken"]);
                AsyncClient asyncClient = yooClient.MakeAsync();

                Payment p = await asyncClient.CapturePaymentAsync(payment.Id);
                int orderId = int.Parse(p.Metadata["OrderID"]);
                Order? ord = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
                ord.PaymentStatus = "Paid";

                Product[] productIdsInOrder = ord.Lines.Select(cl => cl.Product).ToArray();
                Console.WriteLine("count: " + ord.Lines.Count());
                foreach (Product prod in productIdsInOrder)
                {
                    if (prod.Quantity > 0)
                    {
                        CartLine? line = ord.Lines
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
                repo.SaveOrder(ord);
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
                return Ok();
            }
        }
        return BadRequest();
    }

    public async void CreateYooWebHook(string notificationEvent, string accessToken)
    {
        string url = "https://api.yookassa.ru/v3/webhooks";
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage getResponse = await httpClient.GetAsync(url);

        if (getResponse.IsSuccessStatusCode)
        {
            string responseString = await getResponse.Content.ReadAsStringAsync();
            JObject? responseJson = JObject.Parse(responseString);
            if (responseJson["items"].HasValues == false)
            {
                var content = new
                {
                    @event = "payment." + notificationEvent,
                    url = "https://iparts.me/Braintree/Notification" 
                };
                Console.WriteLine($"Creating WebHook for {notificationEvent}");
                httpClient.DefaultRequestHeaders.Add("Idempotence-Key", Guid.NewGuid().ToString());
                var resp = await httpClient.PostAsJsonAsync(url, content);
                httpClient.DefaultRequestHeaders.Remove("Idempotence-Key");
            }
            else
                Console.WriteLine($"WebHook for {notificationEvent} already exists");
        }
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
            return RedirectToAction("Checkout", new { order });
        }
    }
}
