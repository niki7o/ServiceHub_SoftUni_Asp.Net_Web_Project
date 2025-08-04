using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceHub.Core.Models.Service;

namespace ServiceHub.Core.Models
{
    public class ServiceAllViewModel
    {
        public IEnumerable<ServiceViewModel> Services { get; set; } = new List<ServiceViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalServicesCount { get; set; }

       
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AccessTypes { get; set; } = new List<SelectListItem>();

        
        public string? CurrentCategoryFilter { get; set; }
        public string? CurrentAccessTypeFilter { get; set; }
        public string? CurrentSort { get; set; }
        public string? CurrentFilter { get; set; }
    }
}