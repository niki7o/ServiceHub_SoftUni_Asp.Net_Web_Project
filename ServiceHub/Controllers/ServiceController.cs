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
using ServiceHub.Data.DataSeeder;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServiceHub.Controllers
{
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
            IRepository<Favorite> favoriteRepo, IRepository<Service> serviceRepository, IRepository<Category> categoryRepository, ILogger<ServiceController> logger)
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
                else
                {
                    // For now, it will simply ignore the invalid filter.
                }
            }

            var services = await servicesQuery.ToListAsync();

            var serviceDisplayDtos = services.Select(s => new ServiceSeedModel
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
                        canUseService = service.IsBusinessOnly; // This line seems to override the previous `true`
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
            else if (id == ServiceConstants.CodeSnippetConverterServiceId)
            {
                // UPDATED: Supported languages for Code Snippet Converter
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
            /*
            else if (serviceId == ServiceConstants.AiGrammarStyleCheckerServiceId)
            {
                request = new AiGrammarStyleCheckerRequest
                {
                    ServiceId = serviceId,
                    Text = form["text"].ToString(),
                    Language = form["language"].ToString()
                };
            }
            else if (serviceId == ServiceConstants.AiDocumentSummarizerServiceId)
            {
                var file = form.Files.GetFile("fileContent");
                string textContent = form["textContent"].ToString();

                byte[]? fileBytes = null;
                if (file != null)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                request = new AiDocumentSummarizerRequest
                {
                    ServiceId = serviceId,
                    FileContent = fileBytes,
                    FileName = file?.FileName,
                    TextContent = textContent,
                    SummaryType = form["summaryType"].ToString(),
                    SummaryLength = form["summaryLength"].ToString()
                };
            }
            else if (serviceId == ServiceConstants.FinancialCalculatorAnalyzerServiceId)
            {
                var inputDataJson = form["inputDataJson"].ToString();
                var inputData = JsonSerializer.Deserialize<Dictionary<string, string>>(inputDataJson);

                request = new FinancialCalculatorAnalyzerRequest
                {
                    ServiceId = serviceId,
                    CalculationType = form["calculationType"].ToString(),
                    InputData = inputData ?? new Dictionary<string, string>()
                };
            }
            else if (serviceId == ServiceConstants.ContractGeneratorServiceId)
            {
                var contractDataJson = form["contractDataJson"].ToString();
                var contractData = JsonSerializer.Deserialize<Dictionary<string, string>>(contractDataJson);

                request = new ContractGeneratorRequest
                {
                    ServiceId = serviceId,
                    ContractType = form["contractType"].ToString(),
                    ContractData = contractData ?? new Dictionary<string, string>()
                };
            }
            else if (serviceId == ServiceConstants.WebPolicyGeneratorServiceId)
            {
                var policyOptionsJson = form["policyOptionsJson"].ToString();
                var policyOptions = JsonSerializer.Deserialize<Dictionary<string, string>>(policyOptionsJson);

                request = new WebPolicyGeneratorRequest
                {
                    ServiceId = serviceId,
                    PolicyType = form["policyType"].ToString(),
                    CompanyName = form["companyName"].ToString(),
                    CompanyAddress = form["companyAddress"].ToString(),
                    CompanyEmail = form["companyEmail"].ToString(),
                    PolicyOptions = policyOptions ?? new Dictionary<string, string>()
                };
            }
            else if (serviceId == ServiceConstants.InvoiceFactureGeneratorServiceId)
            {
                var sellerJson = form["sellerJson"].ToString();
                var buyerJson = form["buyerJson"].ToString();
                var itemsJson = form["itemsJson"].ToString();

                var seller = JsonSerializer.Deserialize<PartyDetails>(sellerJson);
                var buyer = JsonSerializer.Deserialize<PartyDetails>(buyerJson);
                var items = JsonSerializer.Deserialize<List<InvoiceItem>>(itemsJson);

                request = new InvoiceFactureGeneratorRequest
                {
                    ServiceId = serviceId,
                    InvoiceNumber = form["invoiceNumber"].ToString(),
                    InvoiceDate = DateTime.TryParse(form["invoiceDate"], out var iDate) ? iDate : DateTime.UtcNow,
                    DueDate = DateTime.TryParse(form["dueDate"], out var dDate) ? dDate : DateTime.UtcNow.AddDays(30),
                    Seller = seller!,
                    Buyer = buyer!,
                    Items = items ?? new List<InvoiceItem>(),
                    Currency = form["currency"].ToString(),
                    Notes = form["notes"].ToString()
                };
            }
            else if (serviceId == ServiceConstants.CvResumeGeneratorServiceId)
            {
                var workExperiencesJson = form["workExperiencesJson"].ToString();
                var educationHistoryJson = form["educationHistoryJson"].ToString();
                var skillsJson = form["skillsJson"].ToString();
                var languagesJson = form["languagesJson"].ToString();

                request = new CvResumeGeneratorRequest
                {
                    ServiceId = serviceId,
                    FullName = form["fullName"].ToString(),
                    Email = form["email"].ToString(),
                    PhoneNumber = form["phoneNumber"].ToString(),
                    LinkedInProfile = form["linkedInProfile"].ToString(),
                    Summary = form["summary"].ToString(),
                    WorkExperiences = JsonSerializer.Deserialize<List<WorkExperience>>(workExperiencesJson) ?? new List<WorkExperience>(),
                    EducationHistory = JsonSerializer.Deserialize<List<Education>>(educationHistoryJson) ?? new List<Education>(),
                    Skills = JsonSerializer.Deserialize<List<string>>(skillsJson) ?? new List<string>(),
                    Languages = JsonSerializer.Deserialize<List<string>>(languagesJson) ?? new List<string>(),
                    TemplateStyle = form["templateStyle"].ToString()
                };
            }
            else if (serviceId == ServiceConstants.CodeSnippetConverterServiceId)
            {
                request = new CodeSnippetConverterRequest
                {
                    ServiceId = serviceId,
                    SourceCode = form["sourceCode"].ToString(),
                    SourceLanguage = form["sourceLanguage"].ToString(),
                    TargetLanguage = form["targetLanguage"].ToString()
                };
            }
            else if (serviceId == ServiceConstants.MarketingSloganGeneratorServiceId)
            {
                request = new MarketingSloganGeneratorRequest
                {
                    ServiceId = serviceId,
                    Keywords = form["keywords"].ToString(),
                    NumberOfSlogans = int.TryParse(form["numberOfSlogans"], out var count) ? count : 3,
                    Language = form["language"].ToString()
                };
            }
            */
            else // Ако ServiceId не е разпознат (тъй като другите са коментирани)
            {
                _logger.LogWarning($"Attempted to execute an unknown service with ID: {serviceId}");
                return NotFound($"Услуга с ID '{serviceId}' не е разпозната.");
            }

            if (request == null)
            {
                _logger.LogError("Failed to construct service request object.");
                return BadRequest("Неуспешно създаване на заявка за услуга.");
            }

            if (!TryValidateModel(request))
            {
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
                /*
                else if (response is ContractGeneratorResult contractResult)
                {
                    return File(contractResult.GeneratedFileContent, contractResult.ContentType, contractResult.GeneratedFileName);
                }
                else if (response is InvoiceFactureGeneratorResult invoiceResult)
                {
                    return File(invoiceResult.GeneratedFileContent, invoiceResult.ContentType, invoiceResult.GeneratedFileName);
                }
                else if (response is CvResumeGeneratorResult cvResult)
                {
                    return File(cvResult.GeneratedFileContent, cvResult.ContentType, cvResult.GeneratedFileName);
                }
                else if (response is WebPolicyGeneratorResult webPolicyResult)
                {
                    if (webPolicyResult.GeneratedFileContent != null && !string.IsNullOrEmpty(webPolicyResult.GeneratedFileName))
                    {
                        return File(webPolicyResult.GeneratedFileContent, webPolicyResult.PolicyFormat, webPolicyResult.GeneratedFileName);
                    }
                    return Ok(webPolicyResult);
                }
                */
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