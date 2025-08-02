using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Core.Models.Tools;

using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Diagnostics;
using System.Security.Claims;


namespace ServiceHub.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IServiceService serviceService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Favorite> favoriteRepo;
        private readonly IRepository<Service> serviceRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly ILogger<ServiceController> _logger;
        private readonly IServiceDispatcher _serviceDispatcher;

        public ServiceController(
            IServiceDispatcher serviceDispatcher,
            IServiceService serviceService,
            UserManager<ApplicationUser> userManager,
            IRepository<Favorite> favoriteRepo,
            IRepository<Service> serviceRepository,
            IRepository<Category> categoryRepository,
            ILogger<ServiceController> logger)
        {
            _logger = logger;
            _serviceDispatcher = serviceDispatcher;
            this.serviceService = serviceService;
            this.userManager = userManager;
            this.favoriteRepo = favoriteRepo;
            this.serviceRepository = serviceRepository;
            _categoryRepository = categoryRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> All(string? categoryFilter, string? accessTypeFilter, string? filter = null, string? sort = null, int page = 1)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int servicesPerPage = 9;

            ServiceAllViewModel viewModel = await serviceService.GetAllAsync(
                categoryFilter,
                accessTypeFilter,
                filter,
                sort,
                currentUserId,
                page,
                servicesPerPage
            );

            viewModel.CurrentCategoryFilter = categoryFilter;
            viewModel.CurrentAccessTypeFilter = accessTypeFilter;
            viewModel.CurrentSort = sort;
            viewModel.CurrentFilter = filter;

            return View(viewModel);
        }

        [HttpGet("Service/UseService/{id}")]
        public async Task<IActionResult> UseService(Guid id)
        {
            var service = await serviceRepository.All().FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                _logger.LogWarning($"Attempted to use non-existent service with ID: {id}");
                TempData["ErrorMessage"] = "Услугата не е намерена.";
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                TempData["ErrorMessage"] = "Тази услуга е шаблон и все още не е одобрена за използване.";
                return RedirectToAction("Details", "Service", new { id = service.Id });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Моля, влезте в профила си, за да използвате услугата.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            bool isBusinessUser = await userManager.IsInRoleAsync(user, "BusinessUser");
            bool isRegularUser = await userManager.IsInRoleAsync(user, "User");

            bool canUse = false;
            string accessMessage = "";

            if (isAdmin || isBusinessUser)
            {
                canUse = true;
                accessMessage = $"Вие използвате услугата: {service.Title} (Пълен достъп).";
            }
            else if (isRegularUser)
            {
                if (service.AccessType == AccessType.Free || service.AccessType == AccessType.Partial)
                {
                    canUse = true;
                    accessMessage = $"Вие използвате услугата: {service.Title} (Ограничен достъп).";
                }
                else
                {
                    accessMessage = $"Достъп отказан: Трябва да надстроите абонамента си, за да използвате '{service.Title}'.";
                }
            }
            else
            {
                accessMessage = "Достъп отказан: Вашата роля не позволява това действие.";
            }

            if (!canUse)
            {
                _logger.LogWarning($"User {user.UserName} (Roles: {string.Join(",", await userManager.GetRolesAsync(user))}) denied access to service {service.Title} ({service.Id}) due to AccessType: {service.AccessType}.");
                TempData["ErrorMessage"] = accessMessage;
                return RedirectToAction("Details", "Service", new { id = service.Id });
            }

            _logger.LogInformation($"User {user.UserName} is accessing service form for: {service.Title} ({service.Id}). Access type: {service.AccessType}.");
            TempData["ServiceMessage"] = accessMessage;

            if (id == ServiceConstants.FileConverterServiceId)
            {
                ViewBag.SupportedFormats = new List<string> { "pdf", "docx", "txt", "jpg", "png", "xlsx", "csv" };
                return View("~/Views/Service/_FileConverterForm.cshtml");
            }
            else if (id == ServiceConstants.WordCharacterCounterServiceId)
            {
                return View("~/Views/Service/_WordCharacterCounter.cshtml");
            }
            else if (id == ServiceConstants.TextCaseConverterServiceId)
            {
                return View("~/Views/Service/_TextCaseConverter.cshtml");
            }
            else if (id == ServiceConstants.RandomPasswordGeneratorServiceId)
            {
                return View("~/Views/Service/_RandomPasswordGenerator.cshtml");
            }
            else if (id == ServiceConstants.AutoCvResumeServiceId)
            {
                return Redirect("/CvGenerator/CvGeneratorForm");
            }
            else if (id == ServiceConstants.ContractGeneratorServiceId)
            {
                return Redirect("/ContractGenerator/ContractGeneratorForm");
            }
            else if (id == ServiceConstants.InvoiceReceiptGeneratorId)
            {
                return Redirect("/InvoiceGenerator/InvoiceGeneratorForm");
            }
            else if (id == ServiceConstants.FinancialCalculatorAnalyzerId)
            {
                return Redirect("/FinancialCalculator/CalculatorForm");
            }
            else if (id == ServiceConstants.CodeSnippetConverterServiceId)
            {
                ViewBag.SupportedLanguages = new List<string> { "C#", "Python", "JavaScript", "PHP" };
                return View("~/Views/Service/_CodeSnippetConverter.cshtml");
            }
            else
            {
                _logger.LogWarning($"Service {service.Title} ({service.Id}) found and accessible, but no specific form View is configured.");
                TempData["ErrorMessage"] = $"Форма за услуга '{service.Title}' не е налична или не е разпозната.";
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteService(IFormCollection form)
        {
            if (!Guid.TryParse(form["serviceId"], out Guid serviceId))
            {
                _logger.LogWarning("Invalid or missing serviceId in request.");
                return BadRequest("Невалиден или липсващ идентификатор на услугата.");
            }

            BaseServiceRequest? request = null;

            if (serviceId == ServiceConstants.FileConverterServiceId)
            {
                var file = form.Files.GetFile("fileContent");
                if (file == null)
                {
                    return BadRequest("Файлът е задължителен за услугата за конвертиране на файлове.");
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                request = new FileConvertRequest
                {
                    ServiceId = serviceId,
                    FileContent = ms.ToArray(),
                    OriginalFileName = file.FileName,
                    TargetFormat = form["targetFormat"].ToString(),
                    PerformOCRIfApplicable = bool.TryParse(form["performOCRIfApplicable"], out var ocr) && ocr
                };
            }
            else
            {
                _logger.LogWarning($"Attempted to execute an unknown service with ID: {serviceId}");
                return NotFound($"Услуга с ID '{serviceId}' не е разпозната.");
            }

            if (request == null)
            {
                _logger.LogError("Failed to construct service request object.");
                return BadRequest("Неуспешно създаване на заявка за услуга.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage)
                                             .ToList();
                _logger.LogWarning("Model validation failed for service request. Errors: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Dispatching request for service ID: {request.ServiceId}");
            BaseServiceResponse response = await _serviceDispatcher.DispatchAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation($"Service execution successful for ID: {request.ServiceId}");

                if (response is FileConvertResult fileConvertResult)
                {
                    return File(fileConvertResult.ConvertedFileContent, fileConvertResult.ContentType, fileConvertResult.ConvertedFileName);
                }
                return Ok(response);
            }
            else
            {
                _logger.LogError("Service execution failed for ID: {ServiceId}. Error: {ErrorMessage}", request.ServiceId, response.ErrorMessage);
                return BadRequest(response.ErrorMessage ?? "Неизвестна грешка при изпълнение на услугата.");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid serviceId, string? categoryFilter, string? accessTypeFilter, string? filter, string? sort, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try
            {
                await serviceService.ToggleFavorite(serviceId, userId);
                TempData["SuccessMessage"] = "Статусът на любими е променен успешно!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при промяна на любимия статус.";
            }

            return RedirectToAction("All", new { categoryFilter, accessTypeFilter, filter, sort, page });
        }

        [HttpGet]
        [Route("Service/Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var service = await serviceService.GetByIdAsync(id, currentUserId);
                await serviceService.IncrementViewsCount(id);

                // Logic to determine if the "Use Service" button should be visible
                bool canUseService = false;
                if (User.Identity.IsAuthenticated)
                {
                    var user = await userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");
                        bool isBusinessUser = await userManager.IsInRoleAsync(user, "BusinessUser");
                        bool isRegularUser = await userManager.IsInRoleAsync(user, "User");

                        if (isAdmin || isBusinessUser)
                        {
                            canUseService = true;
                        }
                        else if (isRegularUser)
                        {
                            if (service.AccessType == AccessType.Free || service.AccessType == AccessType.Partial)
                            {
                                canUseService = true;
                            }
                        }
                    }
                }
                ViewBag.CanUseService = canUseService;

                return View(service);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Нямате право да преглеждате този неодобрен шаблон.";
                return RedirectToAction("All");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new ServiceFormModel
            {
                Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Невалидни данни за услугата. Моля, проверете въведеното.";
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Неупълномощен достъп. Моля, влезте в профила си.";
                return Unauthorized();
            }

            try
            {
                await serviceService.CreateAsync(model, adminId);
                TempData["SuccessMessage"] = "Услугата е създадена успешно!";
                return RedirectToAction("All");
            }
            catch (Exception)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Възникна грешка при създаване на услугата.";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "BusinessUser,Admin")]
        public async Task<IActionResult> CreateTemplate()
        {
            var model = new ServiceFormModel
            {
                Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "BusinessUser,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTemplate(ServiceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Невалидни данни за шаблона. Моля, проверете въведеното.";
                return View("CreateTemplate", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Неупълномощен достъп. Моля, влезте в профила си.";
                return Unauthorized();
            }

            var currentUser = await userManager.GetUserAsync(User);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

            try
            {
                await serviceService.AddServiceTemplateAsync(model, userId, isAdmin);
                if (isAdmin)
                {
                    TempData["SuccessMessage"] = "Услугата е създадена успешно (директно одобрена като администратор)!";
                }
                else
                {
                    TempData["SuccessMessage"] = "Шаблонът за услуга е изпратен за одобрение!";
                }
                return RedirectToAction("All");
            }
            catch (InvalidOperationException ex)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = ex.Message;
                return View("CreateTemplate", model);
            }
            catch (Exception)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Възникна грешка при създаване на шаблона.";
                return View("CreateTemplate", model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var currentUser = await userManager.GetUserAsync(User);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

            try
            {
                var model = await serviceService.GetServiceForEditAsync(id, userId, isAdmin);
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                return View(model);
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Нямате право да редактирате тази услуга.";
                return Forbid();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при извличане на услугата за редактиране.";
                return RedirectToAction("All");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ServiceFormModel model)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var currentUser = await userManager.GetUserAsync(User);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Невалидни данни за услугата. Моля, проверете въведеното.";
                return View(model);
            }

            try
            {
                await serviceService.UpdateAsync(id, model, userId, isAdmin);
                TempData["SuccessMessage"] = "Услугата е редактирана успешно!";
                return RedirectToAction("Details", new { id = id });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Нямате право да редактирате тази услуга.";
                return Forbid();
            }
            catch (Exception)
            {
                model.Categories = await _categoryRepository.All().OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
                TempData["ErrorMessage"] = "Възникна грешка при редактиране на услугата.";
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var currentUser = await userManager.GetUserAsync(User);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

            try
            {
                await serviceService.DeleteAsync(id, userId, isAdmin);
                TempData["SuccessMessage"] = "Услугата е изтрита успешно!";
                return RedirectToAction("All");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("All");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при изтриване на услугата.";
                return RedirectToAction("All");
            }
        }
    }
}
