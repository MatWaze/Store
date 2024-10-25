using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Models.ViewModels;

namespace Store.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private UserManager<ApplicationUser> usrMgr;
        private IOrderRepository orderRepo;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository)
        {
            usrMgr = userManager;
            orderRepo = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser? applicationUser = await usrMgr.GetUserAsync(User);

            ProfileViewModel profile = new ProfileViewModel()
            {
                Address = new AddressViewModel
                {
                    City = applicationUser?.City,
                    Country = applicationUser?.Country,
                    Region = applicationUser?.Region,
                    Street = applicationUser?.Street,
                    PostalCode = applicationUser?.PostalCode
                },
                BasicInfo = new BasicInfoViewModel
                {
                    FullName = applicationUser?.FullName,
                    UserName = applicationUser?.UserName,
                    Email = applicationUser?.Email
                },
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == applicationUser.Id)
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
                    City = currentUser?.City,
                    Country = currentUser?.Country,
                    Region = currentUser?.Region,
                    Street = currentUser?.Street,
                    PostalCode = currentUser?.PostalCode
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
            
            if (ModelState.IsValid)
            {
                user.Country = profile.Address.Country;
                user.Region = profile.Address.Region;
                user.City = profile.Address.City;
                user.Street = profile.Address.Street;
                user.PostalCode = profile.Address.PostalCode;

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
                Address = new AddressViewModel
                {
                    City = user?.City,
                    Country = user?.Country,
                    Region = user?.Region,
                    Street = user?.Street,
                    PostalCode = user?.PostalCode
                },
                Orders = orderRepo.Orders
                   .Where(o => o.UserId == user.Id)
                   .ToList()
            });
        }
    }
}
