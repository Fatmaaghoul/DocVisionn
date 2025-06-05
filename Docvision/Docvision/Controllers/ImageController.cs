using Back.Controllers;
using Docvision.Models;
using Docvision.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace doc.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : Controller
    {
        private readonly DocContext _docContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        public ImageController(DocContext docContext, IHttpClientFactory httpClientFactory, ILogger<ImageController> logger)
        {
            _docContext = docContext;
            _httpClient = httpClientFactory.CreateClient(nameof(ImageController));
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var images = await _docContext.Images.ToListAsync();

            if (images == null || !images.Any())
            {
                return NotFound("No images found for the given document ID");
            }

            return Ok(images);

        }
            [HttpGet("descriptions/{imageId}")]
        public async Task<ActionResult> GetDescriptionsByImageId(Guid imageId)
        {
            try
            {
                // Check if the image exists
                var imageExists = await _docContext.Images
                    .AnyAsync(i => i.Id == imageId);

                if (!imageExists)
                {
                    _logger.LogWarning("Image avec ID {ImageId} non trouvée.", imageId);
                    return NotFound($"Image avec ID {imageId} non trouvée.");
                }

                // Fetch descriptions associated with the image
                var descriptions = await _docContext.Descriptions
                    .Where(d => d.DescriptionObjects.Any(doj => doj.DetectedObject.ImageId == imageId))
                    .Include(d => d.DescriptionObjects)
                        .ThenInclude(doj => doj.DetectedObject)
                    .ToListAsync();

                if (descriptions == null || !descriptions.Any())
                {
                    _logger.LogInformation("Aucune description trouvée pour l'image avec ID {ImageId}.", imageId);
                    return Ok(new
                    {
                        Message = "Aucune description trouvée pour cette image.",
                        Descriptions = new List<object>()
                    });
                }

                // Format the response
                var result = descriptions.Select(d => new
                {
                    d.Id,
                    d.text,
                    d.Created,
                    d.Updated,
                    Objects = d.DescriptionObjects.Select(doj => new
                    {
                        doj.DetectedObject.Id,
                        doj.DetectedObject.Name
                    }).ToList()
                }).ToList();

                return Ok(new
                {
                    Message = "Descriptions récupérées avec succès.",
                    Descriptions = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur serveur lors de la récupération des descriptions pour imageId {ImageId}.", imageId);
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }



        [HttpGet("{docId}")]
        public async Task<IActionResult> GetAllImages(Guid docId)
        {

            var images = await _docContext.Images
                .Where(i => i.DocumentId == docId)
                .ToListAsync();

            if (images == null || !images.Any())
            {
                return NotFound("No images found for the given document ID");
            }

            return Ok(images);
        }

        [HttpGet("objects/{imageId}")]
        public async Task<ActionResult<IEnumerable<ObjectImage>>> GetObjectsByImageId(Guid imageId)
        {
            var objects = await _docContext.Objects
                .Where(o => o.ImageId == imageId)
                .Include(o => o.DescriptionObjects) // facultatif si tu veux les descriptions
                .ToListAsync();

            if (objects == null || !objects.Any())
            {
                return NotFound($"Aucun objet trouvé pour l'image avec ID {imageId}");
            }

            return Ok(objects);
        }
        [HttpPost("describe/{imageId}")]
        public async Task<ActionResult> Describe(Guid imageId, [FromBody] DescribeRequest request)
        {
            try
            {
                // Validation des entrées
                if (string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    return BadRequest("L'URL de l'image est requise");
                }

                if (request.Objects == null || !request.Objects.Any())
                {
                    return BadRequest("La liste des objets ne peut pas être vide");
                }

                // Vérifier que l'image existe
                var image = await _docContext.Images.FindAsync(imageId);
                if (image == null)
                {
                    return NotFound($"Image avec ID {imageId} non trouvée");
                }

                // // Récupérer le modèle courant
                // var currentModel = await _docContext.ModelConfigurations
                //     .OrderByDescending(m => m.UpdatedAt)
                //     .FirstOrDefaultAsync();
                // var modelToUse = currentModel?.ModelValue ?? "llava:7b";
                var model = await _docContext.ModelAIs
    .Where(m => m.IsActive == true)
    .FirstOrDefaultAsync();

if (model == null)
{
    return NotFound("Modèle non trouvé");
}

                // Appel à l'API Python
                var payload = new
                {
                    image_url = request.ImageUrl,
                    objects = request.Objects,
                  //  model = modelToUse
                };

                _logger.LogInformation("Appel à l'API Python pour la description");
                var pythonResponse = await _httpClient.PostAsJsonAsync("http://pdf-api:8000/describe", payload);

                if (!pythonResponse.IsSuccessStatusCode)
                {
                    var errorContent = await pythonResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Erreur de l'API Python: {StatusCode} - {Error}",
                        pythonResponse.StatusCode, errorContent);
                    return StatusCode(500, "Erreur lors de la génération de la description");
                }

                var descriptionResponse = await pythonResponse.Content.ReadFromJsonAsync<DescriptionResponse>();

                // Récupérer les objets correspondants depuis la base
                var objectsToLink = await _docContext.Objects
                    .Where(o => o.ImageId == imageId && request.Objects.Contains(o.Name))
                    .ToListAsync();
                // Création de la description et des relations
                var newDescription = new Description
                {
                    Id = Guid.NewGuid(),
                    text = descriptionResponse.Description,
                    //ModelUsed = descriptionResponse.Model_used,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    ModelAIId = model.Id,
                    DescriptionObjects = objectsToLink.Select(o => new DescriptionObject
                    {
                        ObjectId = o.Id
                    }).ToList()
                };
                
                // model.Descriptions.Add(newDescription);
               

                // Sauvegarde transactionnelle
                using var transaction = await _docContext.Database.BeginTransactionAsync();
                try
                {
                    _docContext.Descriptions.Add(newDescription);
                    await _docContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Erreur lors de la sauvegarde de la description");
                    throw;
                }

                return Ok(new
                {
                    Message = "Description générée avec succès",
                    Description = newDescription.text,
                    ModelUsed = newDescription.ModelUsed,
                    ImageId = imageId,
                    LinkedObjects = objectsToLink.Select(o => o.Name)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans le endpoint Describe");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Erreur interne du serveur",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        // Classes DTO
        public class DescribeRequest
        {
            public string ImageUrl { get; set; }

            public List<string> Objects { get; set; }
        }

        public class DescriptionResponse
        {
            public string Description { get; set; }
            public string Model_used { get; set; }
            public string Status { get; set; }
        }


        /* [HttpPost("describe/{imageId}")]
         public async Task<ActionResult> Describe(Guid imageId, [FromBody] List<Guid> objectIds)
         {
             try
             {
                 // Validate inputs
                 if (objectIds == null || !objectIds.Any())
                 {
                     _logger.LogWarning("La liste des IDs d'objets est vide pour imageId {ImageId}.", imageId);
                     return BadRequest("La liste des IDs d'objets ne peut pas être vide.");
                 }

                 // Fetch the image
                 var image = await _docContext.Images
                     .FirstOrDefaultAsync(i => i.Id == imageId);

                 if (image == null)
                 {
                     _logger.LogWarning("Image avec ID {ImageId} non trouvée.", imageId);
                     return NotFound($"Image avec ID {imageId} non trouvée.");
                 }

                 // Fetch the objects (avoid OPENJSON)
                 var objects = new List<ObjectImage>();
                 foreach (var objectId in objectIds)
                 {
                     var obj = await _docContext.Objects
                         .Where(o => o.Id == objectId && o.ImageId == imageId)
                         .FirstOrDefaultAsync();
                     if (obj != null)
                     {
                         objects.Add(obj);
                     }
                 }

                 if (!objects.Any())
                 {
                     _logger.LogWarning("Aucun objet valide trouvé pour les IDs fournis pour imageId {ImageId}.", imageId);
                     return NotFound("Aucun objet valide trouvé pour les IDs fournis.");
                 }

                 // Prepare the request payload for the Python API
                 var objectNames = objects.Select(o => o.Name).Where(name => !string.IsNullOrEmpty(name)).ToList();
                 if (!objectNames.Any())
                 {
                     _logger.LogWarning("Aucun nom d'objet valide trouvé pour les IDs fournis pour imageId {ImageId}.", imageId);
                     return BadRequest("Aucun nom d'objet valide trouvé pour les IDs fournis.");
                 }

                 var requestBody = new
                 {
                     image_url = image.FileUrl,
                     objects = objectNames
                 };

                 // Call the Python /describe API
                 var jsonPayload = JsonSerializer.Serialize(requestBody);
                 _logger.LogInformation("Appel à l'API /describe avec payload : {Payload}", jsonPayload);
                 var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                 var response = await _httpClient.PostAsync("http://localhost:8000/describe", content);

                 if (!response.IsSuccessStatusCode)
                 {
                     var error = await response.Content.ReadAsStringAsync();
                     _logger.LogError("Erreur lors de l'appel à l'API /describe : {StatusCode} - {Error}", response.StatusCode, error);
                     return StatusCode((int)response.StatusCode, $"Erreur lors de l'appel à l'API de description : {error}");
                 }

                 // Read the description (string)
                 var descriptionText = await response.Content.ReadAsStringAsync();
                 _logger.LogInformation("Description reçue : {Description}", descriptionText);

                 // Create a new Description entry
                 var description = new Description
                 {
                     Id = Guid.NewGuid(),
                     text = descriptionText,
                     Created = DateTime.UtcNow,
                     Updated = DateTime.UtcNow
                 };

                 // Save the Description
                 _docContext.Descriptions.Add(description);
                 try
                 {
                     await _docContext.SaveChangesAsync();
                     _logger.LogInformation("Description enregistrée avec ID {DescriptionId}.", description.Id);
                 }
                 catch (DbUpdateException dbEx)
                 {
                     _logger.LogError(dbEx, "Erreur lors de l'enregistrement de la description pour imageId {ImageId}.", imageId);
                     throw new Exception($"Erreur lors de l'enregistrement de la description : {dbEx.InnerException?.Message ?? dbEx.Message}");
                 }

                 // Create DescriptionObject entries
                 var descriptionObjects = objects.Select(o => new DescriptionObject
                 {
                     DescriptionId = description.Id,
                     ObjectId = o.Id
                 }).ToList();

                 _docContext.DescriptionObjects.AddRange(descriptionObjects);
                 try
                 {
                     await _docContext.SaveChangesAsync();
                     _logger.LogInformation("DescriptionObjects enregistrés pour descriptionId {DescriptionId}.", description.Id);
                 }
                 catch (DbUpdateException dbEx)
                 {
                     _logger.LogError(dbEx, "Erreur lors de l'enregistrement des DescriptionObjects pour descriptionId {DescriptionId}.", description.Id);
                     throw new Exception($"Erreur lors de l'enregistrement des DescriptionObjects : {dbEx.InnerException?.Message ?? dbEx.Message}");
                 }

                 // Return the created description
                 return Ok(new
                 {
                     Message = "Description générée et enregistrée avec succès.",
                     Description = new
                     {
                         description.Id,
                         description.text,
                         description.Created,
                         description.Updated,
                         Objects = objects.Select(o => new
                         {
                             o.Id,
                             o.Name
                         })
                     }
                 });
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Erreur serveur lors de la description pour imageId {ImageId}.", imageId);
                 return StatusCode(500, $"Erreur serveur : {ex.Message}");
             }
         }
 */
    }
}
