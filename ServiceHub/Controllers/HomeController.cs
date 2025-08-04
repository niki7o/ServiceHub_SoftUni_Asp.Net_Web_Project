using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models;
using ServiceHub.Data.Models;
using System.Diagnostics;

namespace ServiceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        { // for testing purposes
            //  throw new InvalidOperationException("Това е тестова вътрешна сървърна грешка. Моля, игнорирайте я.");
            return View();

        }

        public async Task<IActionResult> Plans()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return View(currentUser);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
