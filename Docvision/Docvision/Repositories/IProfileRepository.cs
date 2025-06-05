using Docvision.Models;
using Microsoft.AspNetCore.Identity;

namespace Docvision.Repositories
{
    public interface IProfileRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
    }
}