using static ServiceHub.Services.Interfaces.IServiceService;

namespace ServiceHub.Areas.Admin.Services.Interface
{
    public interface IUserService
    {
        Task<PaginatedUsersResult> GetAllUsersAsync(int pageNumber, int pageSize);
    }
}
