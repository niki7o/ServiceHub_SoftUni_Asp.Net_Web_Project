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
using ServiceHub.Data.DataSeeder;
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

        public async Task<IActionResult> All(string? categoryFilter, string? accessTypeFilter)
        {
            var allCategories = await _categoryRepository.All().OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(allCategories, "Name", "Name", categoryFilter);

            var allAccessTypes = Enum.GetNames(typeof(AccessType))
                                     .Select(name => new SelectListItem { Value = name, Text = name })
                                     .ToList();
            ViewBag.AccessTypes = new SelectList(allAccessTypes, "Value", "Text", accessTypeFilter);

            ViewBag.CurrentCategory = categoryFilter;
            ViewBag.CurrentAccessType = accessTypeFilter;

            IQueryable<Service> servicesQuery = serviceRepository.All().Include(s => s.Category);

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                servicesQuery = servicesQuery.Where(s => s.Category != null && s.Category.Name == categoryFilter);
            }

            if (!string.IsNullOrEmpty(accessTypeFilter))
            {
                if (Enum.TryParse<AccessType>(accessTypeFilter, true, out AccessType parsedAccessType))
                {
                    servicesQuery = servicesQuery.Where(s => s.AccessType == parsedAccessType);
                }
            }

            var services = await servicesQuery.ToListAsync();

            var serviceDisplayDtos = services.Select(s => new ServiceSeedModel // ServiceSeedModel е от DataSeeder
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Category = s.Category != null ? s.Category.Name : "N/A",
                AccessType = s.AccessType.ToString(),
             
            }).ToList();

            return View(serviceDisplayDtos);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var service = await serviceRepository.All()
                                                 .Include(s => s.Category)
                                                 .Include(s => s.Reviews)
                                                     .ThenInclude(r => r.User)
                                                 .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            var serviceViewModel = new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                IsBusinessOnly = service.IsBusinessOnly,
                AccessType = service.AccessType,
                CategoryName = service.Category.Name,
                Reviews = service.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    UserName = r.User.UserName,
                    CreatedOn = r.CreatedOn
                }).ToList(),
                AverageRating = service.Reviews.Any() ? service.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = service.Reviews.Count
            };

            bool canUseService = false;
            if (User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(User);
                if (user != null)
                {
                    bool isAdmin = await userManager.IsInRoleAsync(user, "Admin");
                    bool isBusinessUser = await userManager.IsInRoleAsync(user, "BusinessUser");
                    bool isRegularUser = await userManager.IsInRoleAsync(user, "User");

                    if (isAdmin)
                    {
                        canUseService = true;
                    }
                    else if (isBusinessUser)
                    {
                        canUseService = true;
                        // This logic seems to override canUseService based on IsBusinessOnly.
                        // If a BusinessUser should always have access, this line might be problematic.
                        // Keeping it as per your previous code, but noting it.
                        canUseService = service.IsBusinessOnly;
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

            return View(serviceViewModel);
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
                ViewBag.SupportedFormats = new List<string> { "pdf", "docx", "xlsx" };
                ViewBag.ServiceId = id;
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
            else if (id == ServiceConstants.AutoCvResumeServiceId) // Handle CV Generator
            {
                // Пренасочваме към новия CvGeneratorController
                return RedirectToAction("CvGeneratorForm", "CvGenerator");
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
                _logger.LogWarning("Model validation failed for service request. Errors: {Errors}", string.Join("; ", errors));
                return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
            }

            _logger.LogInformation($"Dispatching request for service ID: {request.ServiceId}");
            BaseServiceResponse response = await _serviceDispatcher.DispatchAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation($"Service execution successful for ID: {request.ServiceId}");

                if (response is FileConvertResult fileConvertResult)
                {
                    string fileNameToDownload = Path.GetFileNameWithoutExtension(fileConvertResult.OriginalFileName) + "." + fileConvertResult.TargetFormat;
                    return File(fileConvertResult.ConvertedFileContent, fileConvertResult.ContentType, fileNameToDownload);
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
        public async Task<IActionResult> ToggleFavorite(Guid serviceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var existingFavorite = await favoriteRepo.All()
                                                     .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == serviceId);

            if (existingFavorite != null)
            {
                favoriteRepo.Delete(existingFavorite);
                TempData["SuccessMessage"] = "Removed from favorites!";
            }
            else
            {
                var newFavorite = new Favorite
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    CreatedOn = DateTime.UtcNow
                };
                await favoriteRepo.AddAsync(newFavorite);
                TempData["SuccessMessage"] = "Added to favorites!";
            }
            await favoriteRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = serviceId });
        }
    }
}
