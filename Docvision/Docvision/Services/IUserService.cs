using Docvision.Controllers;
using Docvision.Models;

namespace Docvision.Services
{
    public interface IUserService
    {
        Task<ResponseModel> AddUserAsync(string username, string email, string phonenumber, string password, List<string> roles);
        Task<ResponseModel> EditUserAsync(string userId, EditUserRequest request);
        Task<ResponseModel> DeleteUserAsync(string userId);
        Task<List<UserWithRolesResponse>> GetAllUsersAsync();


    }

    public class UserWithRolesResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }
}
