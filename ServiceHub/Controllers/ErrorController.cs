using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models;
using System.Diagnostics;

namespace ServiceHub.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }


        [Route("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError($"Възникна вътрешна сървърна грешка. Request ID: {requestId}");

            return View("InternalServerError", new ErrorViewModel { RequestId = requestId });
        }

       
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            if (statusCode == 404)
            {
                _logger.LogWarning($"Грешка 404: Ресурсът не е намерен. Път: {HttpContext.Request.Path}");
                return View("NotFound"); 
            }
           
            _logger.LogError($"Възникна HTTP грешка с код: {statusCode}. Път: {HttpContext.Request.Path}");
            return View("InternalServerError", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

  
   

}
