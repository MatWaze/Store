﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Store.Models;
using System.Net.Http.Headers;
using System.Text;
using Yandex.Checkout.V3;
using Microsoft.AspNetCore.Identity;
using Store.Infrastructure;
using System.Security.Cryptography;

namespace Store.Controllers
{
    public class YooKassaController : Controller
    {
        public IOrderRepository repo;
        public HttpClient httpClient;
        public IProductRepository prodRepo;
        public Cart cart;
        private IConfiguration config;
        private UserManager<ApplicationUser> userManager;
        private IRazorViewToStringRenderer razorView;
        private ISendEmail sendEmail;

        public YooKassaController(
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
            repo = repository;
            prodRepo = productRepository;
            cart = cartService;
            userManager = usrMgr;
            config = configuration;
            httpClient = client;
            razorView = razorViewToString;
            sendEmail = send;
        }

        public async Task<IActionResult> YooKassaPayment(int orderId, string accessToken)
        {
            await CreateYooWebHook("waiting_for_capture", "Notification", accessToken);
            //await CreateYooWebHook("succeeded", accessToken);
            await CreateYooWebHook("canceled", "Canceled", accessToken);
            
            ViewBag.ConfirmationToken = await YooToken(orderId, accessToken);
            Console.WriteLine("Confirmation token: " + ViewBag.ConfirmationToken);
            return View("Views/Order/YooKassaCheckout.cshtml", orderId);
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
                Customer = new Customer
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
                return RedirectToAction("YooKassaPayment",
                    new { orderId = orderId, accessToken = user.YooKassaAccessToken });
            }
        }

        public async Task<IActionResult> Callback2(string code)
        {
            string url = "https://yookassa.ru/oauth/v2/token";
            var byteArray = Encoding
                .ASCII
                .GetBytes($"{config["YooKassaApi:ClientId"]}:{Environment.GetEnvironmentVariable("YooKassaApi")}");

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
                return RedirectToAction("YooKassaPayment",
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
            Console.WriteLine($"amount: {amount}");
            var yooReceipt = await CreateYooReceipt();
            var newPayment = new NewPayment
            {
                Amount = new Amount { Value = amount, Currency = "RUB" },
                Confirmation = new Confirmation { Type = ConfirmationType.Embedded },
                Receipt = yooReceipt,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderID", orderId.ToString() },
                    { "AccessToken", accessToken },
                    { "EmailAddress", yooReceipt.Customer.Email }
                },
            };
            Client yooClient = new Client(accessToken: accessToken);
            Payment payment = yooClient.CreatePayment(newPayment);
            //var user = await userManager.GetUserAsync(User);
            order.PaymentId = payment.Id;
            repo.SaveOrder(order);
            var confirmationToken = payment.Confirmation.ConfirmationToken;
            return confirmationToken;
        }

        public async Task<IActionResult> HandleWaitingPayment(PaymentWaitingForCaptureNotification captureNote)
        {
            Payment payment = captureNote.Object;
            Client yooClient = new Client(accessToken: payment.Metadata["AccessToken"]);
            AsyncClient asyncClient = yooClient.MakeAsync();

            if (payment.Paid)
            {
                await asyncClient.CapturePaymentAsync(payment.Id);
            }
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [Route("YooKassa/Canceled")]
        public async Task<IActionResult> Canceled()
        {
            Request.EnableBuffering();

            StringBuilder body = new();
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body.Append(await reader.ReadToEndAsync());
                Request.Body.Position = 0;
                reader.Close();
            }
            Console.WriteLine(body.ToString());

            var notification = Client.ParseMessage(Request.Method,
                Request.ContentType, body.ToString());

            if (notification is PaymentWaitingForCaptureNotification captureNote)
            {
                Payment payment = captureNote.Object;
                Console.WriteLine("Handling cancelation");

                int orderId = int.Parse(payment.Metadata["OrderID"]);
                Order? ord = repo.Orders.FirstOrDefault(o => o.OrderID == orderId);
                
                if (ord != null)
                {
                    Console.WriteLine("canceled");
                    repo.DeleteOrder(ord);

                    ViewResult viewResult = View(payment);
                    viewResult.StatusCode = 200;
                    return viewResult;
                }
            }
            Console.WriteLine("someting else");
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [Route("YooKassa/Notification")]
        public async Task<IActionResult> Notification([FromBody] string body)
        {
            //Request.EnableBuffering();
            //StringBuilder body = new ();
            //using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            //{
            //    body.Append(await reader.ReadToEndAsync());
            //    Request.Body.Position = 0;
            //    reader.Close();
            //}
            Console.WriteLine(body);

            var notification = Client.ParseMessage(Request.Method, 
                Request.ContentType, body);

            if (notification is PaymentWaitingForCaptureNotification captureNote)
            {
                Console.WriteLine("Handling capture");
                return await HandleWaitingPayment(captureNote);
            }
            Console.WriteLine("something else");

            return Ok(); // yookassa will send notes untill status code 200 is sent
        }

        public async Task CreateYooWebHook(string notificationEvent, string path, string accessToken)
        {
            string url = "https://api.yookassa.ru/v3/webhooks";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage getResponse = await httpClient.GetAsync(url);

            if (getResponse.IsSuccessStatusCode)
            {
                string responseString = await getResponse.Content.ReadAsStringAsync();
                JObject? responseJson = JObject.Parse(responseString);

                if (responseJson["items"]?.Count() < 2)
                {
                    var content = new
                    {
                        @event = "payment." + notificationEvent,
                        url = $"https://iparts.me/YooKassa/{path}"
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
    }
}
