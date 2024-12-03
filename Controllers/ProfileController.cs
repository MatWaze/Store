using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models;
using Store.Models.ViewModels;

namespace Store.Controllers
{
    [Authorize(Roles = "ConfirmedUsers")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> usrMgr;
        private readonly IOrderRepository orderRepo;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository
        )
        {
            usrMgr = userManager;
            orderRepo = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var appUser = await usrMgr.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == usrMgr.GetUserId(User));

            ProfileViewModel profile = new ProfileViewModel()
            {
                Address = appUser.Address,
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
            var currentUser = await usrMgr.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == usrMgr.GetUserId(User));

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
                Address = currentUser.Address,
                Orders = orderRepo.Orders
                    .Where(o => o.UserId == currentUser.Id)
                    .ToList()
            });
        }

        [HttpPost("SaveAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAddress(ProfileViewModel profile)
        {
            var user = await usrMgr.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(u => u.Id == usrMgr.GetUserId(User));

            if (ModelState.IsValid)
            {
                user.Address = profile.Address;
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
                Address = user.Address,
                Orders = orderRepo.Orders
                   .Where(o => o.UserId == user.Id)
                   .ToList()
            });
        }
    }
}
