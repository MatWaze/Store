using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models;
using Store.Models.ViewModels;

namespace Store.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> usrMgr;
        private readonly IOrderRepository orderRepo;
        private readonly IdentityContext context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository,
            IdentityContext ctx
        )
        {
            usrMgr = userManager;
            orderRepo = orderRepository;
            context = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var appUser = await usrMgr.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == usrMgr.GetUserId(User));

            ProfileViewModel profile = new ProfileViewModel()
            {
                Address = new AddressViewModel
                {
                    City = appUser?.Address?.City ?? null,
                    Country = appUser?.Address?.Country ?? null,
                    Region = appUser?.Address?.Region ?? null,
                    Street = appUser?.Address?.Street ?? null,
                    PostalCode = appUser?.Address?.PostalCode ?? null
                },
                BasicInfo = new BasicInfoViewModel
                {
                    FullName = appUser?.FullName,
                    UserName = appUser?.UserName,
                    Email = appUser?.Email
                },
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == appUser.Id)
                    .ToList()
            };
            ViewBag.dataSource = profile.Orders;
            return View(profile);
        }

        [HttpPost("SaveBasicInfo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBasicInfo(ProfileViewModel prof)
        {
            ApplicationUser? currentUser = await usrMgr.GetUserAsync(User);
            
            if (ModelState.IsValid)
            {
                currentUser.FullName = prof.BasicInfo.FullName;
                currentUser.UserName = prof.BasicInfo.UserName;
                currentUser.Email = prof.BasicInfo.Email;

                var result = await usrMgr.UpdateAsync(currentUser);
            }

            var addr = context
              .Addresses
              .FirstOrDefault(a => a.Id == currentUser.AddressId);

            return RedirectToAction("Index", new ProfileViewModel
            {
                BasicInfo = new BasicInfoViewModel 
                { 
                    FullName = currentUser.FullName,
                    UserName = currentUser.UserName,
                    Email = currentUser.Email,
                },
                Address = new AddressViewModel
                {
                    City = addr?.City ?? null,
                    Country = addr?.Country ?? null,
                    Region = addr?.Region ?? null,
                    Street = addr?.Street ?? null,
                    PostalCode = addr?.PostalCode ?? null
                },
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == currentUser.Id)
                    .ToList()
            });
        }

        [HttpPost("SaveAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAddress(ProfileViewModel profile)
        {
            ApplicationUser? user = await usrMgr.GetUserAsync(User);
            
            var addr = context
                .Addresses
                .FirstOrDefault(a => a.Id == user.AddressId);

            if (ModelState.IsValid)
            {
                addr.City = profile.Address.City;
                addr.Country = profile.Address.Country;
                addr.PostalCode = profile.Address.PostalCode;
                addr.Region = profile.Address.Region;
                addr.Street = profile.Address.Street;

                context.SaveChanges();

                await usrMgr.UpdateAsync(user);
            }
            return RedirectToAction("Index", new ProfileViewModel
            {
                BasicInfo = new BasicInfoViewModel
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                },
                Address = addr,
                Orders = orderRepo.Orders
                   .Where(o => o.UserId == user.Id)
                   .ToList()
            });
        }
    }
}
