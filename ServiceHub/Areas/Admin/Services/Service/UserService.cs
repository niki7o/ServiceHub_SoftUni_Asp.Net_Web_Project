using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.Admin.Models;
using ServiceHub.Areas.Admin.Services.Interface;
using ServiceHub.Data.Models;

namespace ServiceHub.Areas.Admin.Services.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger; 

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<PaginatedUsersResult> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            _logger.LogInformation($"GetAllUsersAsync: Fetching users for page {pageNumber} with page size {pageSize}.");

            var query = _userManager.Users.AsNoTracking();

            int totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserName) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            _logger.LogInformation($"GetAllUsersAsync: Retrieved {userViewModels.Count} users out of {totalCount} total.");

            return new PaginatedUsersResult
            {
                Users = userViewModels,
                TotalCount = totalCount
            };
        }
    }
}
