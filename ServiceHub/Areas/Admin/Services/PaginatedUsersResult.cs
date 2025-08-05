using ServiceHub.Areas.Admin.Models;

namespace ServiceHub.Areas.Admin.Services
{
    public class PaginatedUsersResult
    {
        public IEnumerable<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public int TotalCount { get; set; }
    }
}
