using Docvision.Models;
using Microsoft.AspNetCore.Identity;

namespace Docvision.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(ApplicationUser user);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<List<string>> GetUserRolesAsync(ApplicationUser user);
        Task<IdentityResult> AddRolesToUserAsync(ApplicationUser user, List<string> roles);
        Task<IdentityResult> RemoveRolesFromUserAsync(ApplicationUser user, List<string> roles);
    }

}
