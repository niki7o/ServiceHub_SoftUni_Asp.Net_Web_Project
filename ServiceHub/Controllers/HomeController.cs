using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models;
using System.Diagnostics;

namespace ServiceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //  throw new InvalidOperationException("Това е тестова вътрешна сървърна грешка. Моля, игнорирайте я.");
            return View();

        }

        public IActionResult Plans()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
