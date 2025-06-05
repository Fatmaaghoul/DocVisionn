using System;
using System.Threading.Tasks;
using CloudinaryDotNet;
using Docvision.Models;
using Docvision.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/admin/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class DocumentAdminController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly Cloudinary _cloudinary;

        public DocumentAdminController(IDocumentService documentService, Cloudinary cloudinary)
        {
            _documentService = documentService;
            _cloudinary = cloudinary;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // GET: All documents
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // if (!IsAdmin()) return Forbid();

            var docs = await _documentService.GetAllDocumentsAsync();
            return Ok(docs);
        }

        // GET: Document by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // if (!IsAdmin()) return Forbid();

            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            return Ok(doc);
        }

        // POST: Create a document
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateDocumentRequest request)
        {
            var AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(AdminId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }
            // if (!IsAdmin()) return Forbid();

            // Validate input
            if (request.File == null || request.File.Length == 0)
            {
                ModelState.AddModelError("File", "The file field is required.");
            }
            else if (request.File.ContentType != "application/pdf")
            {
                ModelState.AddModelError("File", "Only PDF files are allowed.");
            }
            else if (request.File.Length > 10 * 1024 * 1024) // 10 MB limit
            {
                ModelState.AddModelError("File", "File size exceeds the 10 MB limit.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                ModelState.AddModelError("Name", "The name field is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                ModelState.AddModelError("Description", "The description field is required.");
            }
            if (string.IsNullOrWhiteSpace(request.UserId) || !Guid.TryParse(request.UserId, out _))
            {
                ModelState.AddModelError("UserId", "A valid userId (GUID) is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _documentService.AddDocumentAsync(request.File, request.Name, request.Description, request.UserId,AdminId);
                return Ok(created);
            }
            catch (Exception ex)
            {
                // Log the exception (use a logging framework in production)
                Console.WriteLine($"Error creating document: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while creating the document." });
            }
        }

        // PUT: Update document name/description
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest model)
        {
            // if (!IsAdmin()) return Forbid();

            // Validate input
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "The name field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError("Description", "The description field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.UserId) || !Guid.TryParse(model.UserId, out _))
            {
                ModelState.AddModelError("UserId", "A valid userId (GUID) is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _documentService.UpdateDocumentAsync(id, model.Name, model.Description, model.UserId);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating document: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while updating the document." });
            }
        }

        // DELETE: Remove a document
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // if (!IsAdmin()) return Forbid();

            try
            {
                var deleted = await _documentService.DeleteDocumentAsync(id);
                if (!deleted) return NotFound();
                return Ok("Document supprimé avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting document: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while deleting the document." });
            }
        }

        // GET: User by DocumentId
        [HttpGet("{id}/user")]
        public async Task<IActionResult> GetUserByDocumentId(Guid id)
        {
            // if (!IsAdmin()) return Forbid();

            try
            {
                var user = await _documentService.GetUserByDocumentIdAsync(id);
                if (user == null) return NotFound("Utilisateur non trouvé pour ce document.");
                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user by document ID: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while fetching the user." });
            }
        }

        // GET: Cloudinary storage usage
        [HttpGet("storageCloud")]
        public async Task<IActionResult> GetStorageCloud()
        {
            // if (!IsAdmin()) return Forbid();

            try
            {
                var usageResult = await _cloudinary.GetUsageAsync();
                if (usageResult?.Storage == null)
                {
                    return StatusCode(500, "Erreur lors de la récupération des données de stockage Cloudinary : réponse invalide.");
                }

                Console.WriteLine($"Cloudinary Usage - Used: {usageResult.Storage.Used}, Limit: {usageResult.Storage.Limit}");

                var totalStorage = Math.Round(usageResult.Storage.Used / 1073741824.0, 2); // Bytes to GB
                var maxStorage = usageResult.Storage.Limit > 0
                    ? Math.Round(usageResult.Storage.Limit / 1073741824.0, 2)
                    : 10.0; // Default to 10 GB for free plan

                return Ok(new
                {
                    totalStorage,
                    maxStorage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'appel à Cloudinary : {ex.Message}");
                return StatusCode(500, $"Erreur lors de la récupération des données de stockage : {ex.Message}");
            }
        }

        // Request models
        public class CreateDocumentRequest
        {
            [Required]
            public IFormFile File { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Description { get; set; }
            [Required]
            public string UserId { get; set; }
          
        }

        public class UpdateDocumentRequest
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Description { get; set; }
            [Required]
            public string UserId { get; set; }
        }
    }
}