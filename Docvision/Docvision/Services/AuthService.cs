using AutoMapper;
using Docvision.Dtos;
using Docvision.Helpers;
using Docvision.Models;
using Docvision.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Docvision.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly JwtHelper _jwtHelper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;



        public AuthService(
            IAuthRepository authRepository,
            IEmailService emailService,
            IConfiguration configuration,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            JwtHelper jwtHelper,
             RoleManager<IdentityRole> roleManager,
                    UserManager<ApplicationUser> userManager)

        {
            _authRepository = authRepository;
            _emailService = emailService;
            _configuration = configuration;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _jwtHelper = jwtHelper;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        private IUrlHelper GetUrlHelper()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var actionContext = _actionContextAccessor.ActionContext;
            return _urlHelperFactory.GetUrlHelper(actionContext);
        }

        public async Task<ResponseModel> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _authRepository.GetUserByEmailAsync(request.Email);
            if (emailExists != null)
                return new ResponseModel { Success = false, Message = "L'email est déjà utilisé." };

            var userNameExists = await _authRepository.GetUserByEmailAsync(request.UserName);
            if (userNameExists != null)
                return new ResponseModel { Success = false, Message = "Le nom d'utilisateur est déjà pris." };

            var user = _mapper.Map<ApplicationUser>(request);
            user.RefreshToken = "";

            var result = await _authRepository.CreateUserAsync(user, request.Password);
            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            // Vérifiez si le rôle "User" existe, sinon créez-le
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Attribuez le rôle "User" à l'utilisateur
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                return new ResponseModel { Success = false, Message = "Erreur lors de l'attribution du rôle." };
            }

            var confirmationToken = await _authRepository.GenerateEmailConfirmationTokenAsync(user);

            // Modifier cette partie pour pointer vers l'API backend, pas le frontend
            var confirmationLink = $"http://localhost:5000/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

            await _emailService.SendEmailAsync(user.Email, "Confirmez votre e-mail",
                $"Cliquez ici pour confirmer votre e-mail : <a href='{confirmationLink}'>Confirmer</a>");

            return new ResponseModel { Success = true, Message = "Utilisateur créé avec succès. Un e-mail de confirmation a été envoyé." };
        }

        public async Task<ResponseModel> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                Console.WriteLine($"Login attempt for email: {request.Email}");
                Console.WriteLine($"User found: {user != null}");

                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return new ResponseModel { Success = false, Message = "Email ou mot de passe incorrect" };
                }

                var isEmailConfirmed = await _authRepository.IsEmailConfirmedAsync(user);
                var isPasswordValid = await _authRepository.CheckPasswordAsync(user, request.Password);
                
                Console.WriteLine($"Email confirmed: {isEmailConfirmed}");
                Console.WriteLine($"Password check result: {isPasswordValid}");

                if (!isEmailConfirmed)
                {
                    Console.WriteLine("Email not confirmed");
                    return new ResponseModel { Success = false, Message = "Veuillez confirmer votre e-mail avant de vous connecter. Un nouvel e-mail de confirmation a été envoyé." };
                }

                if (!isPasswordValid)
                {
                    Console.WriteLine("Invalid password");
                    return new ResponseModel { Success = false, Message = "Email ou mot de passe incorrect" };
                }

                // Utilisation de la méthode asynchrone GenerateJwtTokenAsync
                var token = await _jwtHelper.GenerateJwtTokenAsync(user);
                var refreshToken = JwtHelper.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                await _authRepository.UpdateUserAsync(user);

                // Ajouter le token dans un cookie
                _httpContextAccessor.HttpContext.Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });

                return new ResponseModel
                {
                    Success = true,
                    Message = "Connexion réussie",
                    Data = new AuthResponse { Token = token, RefreshToken = refreshToken }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new ResponseModel { Success = false, Message = "Une erreur est survenue lors de la connexion" };
            }
        }


               public async Task<ResponseModel> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return new ResponseModel { Success = false, Message = "L'ID utilisateur ou le token est manquant." };

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _authRepository.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "E-mail confirmé avec succès !" };
        }
        public async Task<ResponseModel> ForgotPasswordAsync(string email)
        {
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Email non trouvé." };

            var token = await _authRepository.GeneratePasswordResetTokenAsync(user);

            // Générer le lien de réinitialisation
            var urlHelper = GetUrlHelper();
            var frontendUrl ="http://localhost:8080";
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";

            await _emailService.SendEmailAsync(user.Email, "Réinitialisation de mot de passe",
                $"Cliquez ici pour réinitialiser votre mot de passe : <a href='{resetLink}'>Réinitialiser</a>");

            return new ResponseModel { Success = true, Message = "Un e-mail de réinitialisation a été envoyé !" };
        }

        public async Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Email non trouvé." };

            var decodedToken = Uri.UnescapeDataString(request.Token);
            var result = await _authRepository.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Mot de passe réinitialisé avec succès !" };
        }


        public async Task<ResponseModel> RefreshTokenAsync(string refreshToken)
        {
            var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Refresh token invalide." };

            // Utilisation de la méthode asynchrone GenerateJwtTokenAsync
            var token = await _jwtHelper.GenerateJwtTokenAsync(user); // Appel asynchrone

            // Générer un nouveau refresh token
            var newRefreshToken = JwtHelper.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authRepository.UpdateUserAsync(user);

            return new ResponseModel
            {
                Success = true,
                Message = "Token rafraîchi avec succès.",
                Data = new AuthResponse { Token = token, RefreshToken = newRefreshToken }
            };
        }

    }
}
