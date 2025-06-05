using Docvision.Dtos;
using Docvision.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Docvision.Services
{
    public interface IAuthService
    {
        Task<ResponseModel> RegisterAsync(RegisterRequest request);
        Task<ResponseModel> LoginAsync(LoginRequest request);
        Task<ResponseModel> ConfirmEmailAsync(string userId, string token);
        Task<ResponseModel> ForgotPasswordAsync(string email);
        Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequest request);
       
        Task<ResponseModel> RefreshTokenAsync(string refreshToken);
    }
}