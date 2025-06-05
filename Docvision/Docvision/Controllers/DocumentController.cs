using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docvision.Persistance;
using Docvision.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Docvision.Repositories;
using Docvision.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _idocumentRepository;
        private readonly DocContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DocumentRepository> _logger;


        public DocumentController(IDocumentRepository IdocumentRepository, DocContext docContext, IHttpClientFactory httpClientFactory, ILogger<DocumentRepository> logger)
        {
            _idocumentRepository = IdocumentRepository;
            _context = docContext;
            _httpClient = httpClientFactory.CreateClient(nameof(DocumentController)); 
            _logger = logger;

        }
        [HttpPost("analyze/{id}")]
        public async Task<IActionResult> AnalyzeDocument(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var document = await _context.Documents
                    .Include(d => d.Images)
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (document == null)
                    return NotFound("Document non trouvé ou vous n'avez pas les droits d'accès.");

                if (!document.isExtracted)
                    return BadRequest("Le document doit d'abord être extrait avant d'être analysé.");

                // Liste pour collecter tous les objets détectés
                var allDetectedObjects = new HashSet<string>();

                foreach (var image in document.Images)
                {
                    var requestBody = new
                    {
                        image_url = image.FileUrl,
                        text = document.Text
                    };

                    var response = await _httpClient.PostAsJsonAsync("http://pdf-api:8000/analyze", requestBody);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Erreur lors de l'analyse de l'image {ImageId} : {Error}", image.Id, error);
                        continue;
                    }

                    var result = await response.Content.ReadFromJsonAsync<DescribeImageResult>();

                    // Vider les objets existants liés à cette image
                    var existingObjects = await _context.Objects
                        .Where(o => o.ImageId == image.Id)
                        .ToListAsync();
                    _context.Objects.RemoveRange(existingObjects);

                    if (result?.Result != null)
                    {
                        int totalTextOccurrences = result.Result.Sum(r => r.Value.OccurenceText);

                        foreach (var kvp in result.Result)
                        {
                            var objectName = kvp.Key;
                            var occurrenceData = kvp.Value;

                            // S'assurer que le nom de l'objet est une chaîne de caractères
                            string objectNameString = objectName.ToString();
                            
                            // Ajouter l'objet à la collection de tous les objets
                            allDetectedObjects.Add(objectNameString);

                            var pourcentageText = totalTextOccurrences > 0
                                     ? (float)occurrenceData.OccurenceText / totalTextOccurrences * 100
                                     : 0;

                            var imageObject = new ObjectImage
                            {
                                Id = Guid.NewGuid(),
                                Name = objectNameString,
                                OccurenceText = occurrenceData.OccurenceText,
                                OccurenceImage = occurrenceData.OccurenceImage,
                                Pourcentage = pourcentageText,
                                ImageId = image.Id
                            };

                            _context.Objects.Add(imageObject);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Résultat null ou vide pour l'image {ImageId}.", image.Id);
                    }
                }

                // Mettre à jour la liste AllObjects du document
                document.AllObjects = allDetectedObjects.ToList();
                document.isAnalysed = true;
                document.ModifiedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Analyse effectuée avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'analyse du document");
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }





        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] CreateDocumentRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }
                var document = await _idocumentRepository.AddDocumentAsync(request.File, request.Name, request.Description, userId);

                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de l'upload du document : {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Utilisateur non authentifié.");
                }

                var documents = await _idocumentRepository.GetAllDocumentAsync(userId);
                var result = documents.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    UploadDate = d.UploadDate,
                    Description = d.description,
                    IsAnalysed = d.isAnalysed,
                    IsExtracted = d.isExtracted,
                    FileUrl = d.FileUrl,
                    propriétaireId = d.propriétaireId,
                    Text = d.Text,
                    ModifiedAt = d.ModifiedAt,
                    AllObjects = d.AllObjects,
                    Images = d.Images?.Select(i => new DocumentImageDto
                    {
                        Id = i.Id,
                        FileUrl = i.FileUrl,
                       
                    }).ToList() ?? new List<DocumentImageDto>()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _idocumentRepository.GetDocumentByIdAsync(id, userId);
            if (document == null) return NotFound();

            var result = new DocumentDto
            {
                Id = document.Id,
                Name = document.Name,
                UploadDate = document.UploadDate,
                Description = document.description,
                IsAnalysed = document.isAnalysed,
                resumer = document.resumer,
                IsExtracted = document.isExtracted,
                propriétaireId = document.propriétaireId,
                FileUrl = document.FileUrl,
                Text = document.Text,
                ModifiedAt = document.ModifiedAt,
                AllObjects = document.AllObjects,
                Images = document.Images?.Select(i => new DocumentImageDto
                {
                    Id = i.Id,
                    FileUrl = i.FileUrl,

                }).ToList() ?? new List<DocumentImageDto>()
            };

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] DocumentUpdateDto

 updatedDocument)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            var document = await _idocumentRepository.UpdateDocumentAsync(id, updatedDocument, userId);
            if (document == null)
            {
                return NotFound("Document non trouvé ou l'utilisateur n'a pas accès à ce document.");
            }

            return Ok(document);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }
            await _idocumentRepository.DeleteDocumentAsync(id, userId);

            return Ok("Document supprimé avec succès.");
        }
        [HttpPost("extract/{id}")]
        public async Task<IActionResult> Extract(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            try
            {
                // Extraction texte + images (sans analyse)
                var result = await _idocumentRepository.ExtractTextAndImagesAsync(document.FileUrl);

                // Mise à jour du document
                document.Text = result.Text;
                document.isExtracted = true; // Marquer comme extrait
                document.isAnalysed = false;  // Aucune analyse effectuée
                

                // Remplacer les anciennes images
                var oldImages = await _context.Images.Where(i => i.DocumentId == id).ToListAsync();
                _context.Images.RemoveRange(oldImages);

                foreach (var imageUrl in result.ImageUrls)
                {
                    _context.Images.Add(new DocumentImage
                    {
                        Id = Guid.NewGuid(),
                        FileUrl = imageUrl,
                        DocumentId = id,
                       // Description = "Image non analysée",
                        Objects = null
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Extraction texte+images réussie",
                    Text = result.Text,
                    Images = result.ImageUrls
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur d'extraction");
                return StatusCode(500, "Erreur lors de l'extraction");
            }
        }

        [HttpPost("summarize/{id}")]
        public async Task<IActionResult> Summarize(Guid id)
        {
            try
            {
                // Récupérer le document
                var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
                if (document == null)
                    return NotFound("Document non trouvé.");

                // Vérifier si le texte est extrait
                if (string.IsNullOrEmpty(document.Text))
                    return BadRequest("Le document n'a pas de texte extrait. Veuillez d'abord extraire le document.");

                // Préparer la requête pour l'API FastAPI
                var requestBody = new { text = document.Text };
                var response = await _httpClient.PostAsJsonAsync("http://pdf-api:8000/resumer", requestBody);

                // Vérifier la réponse de l'API
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Erreur lors de l'appel à l'API /resumer : {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, $"Erreur lors de la génération du résumé : {errorContent}");
                }

                // Lire la réponse
                var result = await response.Content.ReadFromJsonAsync<SummaryResult>();
                if (result == null || string.IsNullOrEmpty(result.Summary))
                {
                    _logger.LogError("Réponse invalide de l'API /resumer");
                    return StatusCode(500, "Réponse invalide de l'API de résumé.");
                }

                // Mettre à jour le champ resumer du document
                document.resumer = result.Summary;
                document.ModifiedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Retourner le résumé
                return Ok(new
                {
                    Message = "Résumé généré et enregistré avec succès.",
                    Summary = document.resumer
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur réseau lors de l'appel à l'API /resumer");
                return StatusCode(500, $"Erreur réseau : {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la génération du résumé");
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }

        // Classe pour désérialiser la réponse de l'API /resumer
        public class SummaryResult
        {
            public string Summary { get; set; }
        }
        public class ExtractedResult
        {
            public string Text { get; set; }
            public List<string> ImageUrls { get; set; }
        }

        public class DescribeImageResult
        {
            public Dictionary<string, OccurrenceData> Result { get; set; } = new Dictionary<string, OccurrenceData>();
        }

        public class OccurrenceData
        {
            [JsonPropertyName("occurence_text")]
            public int OccurenceText { get; set; }

            [JsonPropertyName("occurence_image")]
            public int OccurenceImage { get; set; }
        }



        public class CreateDocumentRequest
        {
            [Required]
            public IFormFile File { get; set; }
            [Required]
            public string Name { get; set; }
          
             public string Description { get; set; } = "";

        } 
    }
}


