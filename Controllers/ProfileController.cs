using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Store.Models;
using Store.Models.ViewModels;

namespace Store.Controllers
{
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
            ApplicationUser applicationUser = await usrMgr.GetUserAsync(User);

            ProfileViewModel profile = new ProfileViewModel()
            {
                User = applicationUser,
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == applicationUser.Id)
                    .ToList()
            };
            ViewBag.dataSource = profile.Orders;
            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBasicInfo(ProfileViewModel prof)
        {
            ApplicationUser? currentUser = await usrMgr.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                currentUser.FullName = prof.User.FullName;
                currentUser.UserName = prof.User.UserName;
                currentUser.Email = prof.User.Email;

                var result = await usrMgr.UpdateAsync(currentUser);
            }
            
            return View("Index", new ProfileViewModel
            {
                User = currentUser, // Use the submitted usr to persist changes
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == currentUser.Id)
                    .ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAddress(ProfileViewModel profile)
        {
            ApplicationUser? user = await usrMgr.GetUserAsync(User);
            
            if (ModelState.IsValid)
            {
                user.Country = profile.User.Country;
                user.Region = profile.User.Region;
                user.City = profile.User.City;
                user.Street = profile.User.Street;
                user.PostalCode = profile.User.PostalCode;

                await usrMgr.UpdateAsync(user);
            }
            return View("Index", new ProfileViewModel
            {
                User = user, // Use the submitted usr to persist changes
                Orders = orderRepo.Orders
                   .Where(o => o.UserId == user.Id)
                   .ToList()
            });
        }
    }
}
