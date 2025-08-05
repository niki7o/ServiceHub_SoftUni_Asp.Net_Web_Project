using ServiceHub.Core.Models.Service;

namespace ServiceHub.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public IEnumerable<ServiceViewModel> PendingServices { get; set; } = new List<ServiceViewModel>();
        public int UsersCurrentPage { get; set; }
        public int UsersTotalPages { get; set; }
        public int UsersPageSize { get; set; } = 5; 
        public int TotalUsersCount { get; set; }

        
        public int PendingServicesCurrentPage { get; set; }
        public int PendingServicesTotalPages { get; set; }
        public int PendingServicesPageSize { get; set; } = 5; 
        public int TotalPendingServicesCount { get; set; }
    }
}
