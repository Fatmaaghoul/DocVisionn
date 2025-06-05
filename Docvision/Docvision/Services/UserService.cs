using Docvision.Controllers;
using Docvision.Models;
using Docvision.Persistance;
using Docvision.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Docvision.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;




        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;

        }
      

        public async Task<ResponseModel> AddUserAsync(string username, string email, string phonenumber, string password, List<string> roles)
        {
            var user = new ApplicationUser { UserName = username, Email = email, PhoneNumber = phonenumber         ,RefreshToken = string.Empty 
            };
            var result = await _userRepository.CreateUserAsync(user, password);

            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            var roleResult = await _userRepository.AddRolesToUserAsync(user, roles);
            if (!roleResult.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", roleResult.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Utilisateur ajouté avec succès." };
        }

        public async Task<ResponseModel> EditUserAsync(string userId, EditUserRequest request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };

            // Vérifier qu'au moins un champ est modifié (username OU email OU phoneNumber OU roles)
            if (request.Username == null && request.Email == null && request.PhoneNumber == null && request.Roles == null)
            {
                return new ResponseModel { Success = false, Message = "Aucune modification fournie." };
            }

            // Mise à jour conditionnelle des champs
            if (request.Username != null)
                user.UserName = request.Username;

            if (request.Email != null)
                user.Email = request.Email;

            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;

            // Mise à jour de l'utilisateur seulement si au moins un champ est modifié
            if (request.Username != null || request.Email != null || request.PhoneNumber != null)
            {
                var updateResult = await _userRepository.UpdateUserAsync(user);
                if (!updateResult.Succeeded)
                    return new ResponseModel { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
            }

            // Gestion des rôles (si fournis)
            if (request.Roles != null)
            {
                var currentRoles = await _userRepository.GetUserRolesAsync(user);
                var removeResult = await _userRepository.RemoveRolesFromUserAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return new ResponseModel { Success = false, Message = string.Join(", ", removeResult.Errors.Select(e => e.Description)) };

                var addResult = await _userRepository.AddRolesToUserAsync(user, request.Roles);
                if (!addResult.Succeeded)
                    return new ResponseModel { Success = false, Message = string.Join(", ", addResult.Errors.Select(e => e.Description)) };
            }

            return new ResponseModel { Success = true, Message = "Utilisateur modifié avec succès." };
        }
        public async Task<ResponseModel> DeleteUserAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };

            var result = await _userRepository.DeleteUserAsync(user);
            if (!result.Succeeded)
                return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            return new ResponseModel { Success = true, Message = "Utilisateur supprimé avec succès." };
        }
        public async Task<List<UserWithRolesResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userResponses = new List<UserWithRolesResponse>();

            foreach (var user in users)
            {
                var roles = await _userRepository.GetUserRolesAsync(user);
                userResponses.Add(new UserWithRolesResponse
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,

                    Roles = roles
                });
            }

            return userResponses;
        }
    }
}