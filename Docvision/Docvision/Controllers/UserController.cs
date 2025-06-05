using Docvision.Models;
using Docvision.Repositories;
using Docvision.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository, IUserService userService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userService = userService;
            _userManager = userManager;
            _roleManager = roleManager;

        }
        [HttpGet("id")]
        public async Task<IActionResult> GetUserById(string id)
        {
             var user = await _userRepository.GetUserByIdAsync(id);
            return Ok(user);
        }

        // ✅ 1. Ajouter un utilisateur avec un ou plusieurs rôles
        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest model)
        {
            var result = await _userService.AddUserAsync(model.Username, model.Email, model.PhoneNumber, model.Password, model.Roles);
            return Ok(result);
        }

        // ✅ 2. Modifier un utilisateur et ses rôles
        [HttpPut("edit/{userId}")]
        public async Task<IActionResult> EditUser(string userId, [FromBody] EditUserRequest model)
        {
            var result = await _userService.EditUserAsync(userId, model);
            return Ok(result);
        }

        // ✅ 3. Supprimer un utilisateur
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            return Ok(result);
        }

        // ✅ 4. Récupérer tous les utilisateurs avec leurs rôles
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // ✅ 5. Récupérer tous les rôles disponibles
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }


    }

    // Modèles de requête
    public class AddUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }
    }
    public class EditUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? Roles { get; set; }
    }
}