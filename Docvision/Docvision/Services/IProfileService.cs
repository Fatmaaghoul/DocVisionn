using Docvision.Dtos;
using Docvision.Models;

namespace Docvision.Services
{
    public interface IProfileService
    {
        Task<ResponseModel> GetProfileAsync(string userId);
        Task<ResponseModel> EditProfileAsync(string userId, EditProfileRequest request);
    }
}
