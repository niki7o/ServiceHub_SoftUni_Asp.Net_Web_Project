using ServiceHub.Core.Models.Service;

namespace ServiceHub.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public IEnumerable<ServiceViewModel> PendingServices { get; set; } = new List<ServiceViewModel>();
    }
}
