using Docvision.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Docvision.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelAIController : ControllerBase
    {
        private readonly DocContext _context;
        private readonly ILogger<ModelAIController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public ModelAIController(ILogger<ModelAIController> logger, DocContext docContext, IWebHostEnvironment environment)
        {
            _context = docContext;
            _environment = environment;
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://pdf-api:8000")
            };
        }

        // Classes internes
        public class ModelDownloadRequest
        {
            [JsonPropertyName("model_name")]
            public string ModelName { get; set; }
        }

        public class ModelDownloadResponse
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("progress")]
            public int Progress { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
          [JsonPropertyName("model_name")]
             public string ModelName { get; set; }
        }
    public class ModelList
    {
        [JsonPropertyName("model_names")]
        public List<string> ModelNames { get; set; }
    }
        // Fonction pour récupérer les noms de modèles depuis le fichier JSON
private async Task<List<string>> GetValidModelNames()
        {
            try
            {
                // Utiliser IWebHostEnvironment pour obtenir le chemin du fichier dans wwwroot
                string jsonPath = Path.Combine(_environment.ContentRootPath, "Data/ollama_models.json");
                if (!System.IO.File.Exists(jsonPath))
                {
                    _logger.LogWarning("Fichier valid_models.json non trouvé à {Path}", jsonPath);
                    return new List<string>();
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);
                var modelList = JsonSerializer.Deserialize<ModelList>(jsonContent);
                
                return modelList?.ModelNames?.Where(m => !string.IsNullOrWhiteSpace(m)).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la lecture du fichier valid_models.json");
                return new List<string>();
            }
        }
        // POST api/modelai/download
        [HttpPost("download")]
        public async Task<ActionResult<ModelDownloadResponse>> Download([FromBody] ModelDownloadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ModelName))
            {
                return BadRequest(new { status = "error", message = "Le nom du modèle est requis" });
            }
        var validModels = await GetValidModelNames();
        if (!validModels.Contains(request.ModelName))
        {
            return BadRequest(new { status = "error", message = $"Le modèle '{request.ModelName}' n'existe pas dans Ollama" });
        }
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { model_name = request.ModelName }),
                    Encoding.UTF8,
                    "application/json"
                );

                var startResponse = await _httpClient.PostAsync("/models/download", content);
                startResponse.EnsureSuccessStatusCode();

                var startJson = await startResponse.Content.ReadAsStringAsync();
                var startData = JsonSerializer.Deserialize<ModelDownloadResponse>(startJson);

                if (startData == null)
                {
                    return StatusCode(500, new { status = "error", message = "Erreur lors du démarrage du téléchargement" });
                }

                return Ok(new
                {
                    status = startData.Status,
                    message = startData.Message ?? "Téléchargement démarré"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du téléchargement du modèle");
                return StatusCode(500, new { status = "error", message = "Erreur lors du téléchargement du modèle" });
            }
        }
   [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            try
            {
                var response = await _httpClient.PostAsync("/models/cancel-download", null);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<ModelDownloadResponse>(json);

                if (responseData == null)
                {
                    return StatusCode(500, new { status = "error", message = "Échec de la désérialisation de la réponse" });
                }

                return Ok(new { status = responseData.Status ?? "unknown", message = responseData.Message ?? "Téléchargement annulé" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation");
                return StatusCode(500, new { status = "error", message = "Erreur lors de l'annulation du téléchargement" });
            }
        }



       // GET api/modelai/download-status
[HttpGet("download-status")]
public async Task<ActionResult<ModelDownloadResponse>> DownloadStatus()
{
    try
    {
        var response = await _httpClient.GetAsync("/models/download-status");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var statusData = JsonSerializer.Deserialize<ModelDownloadResponse>(json);

        if (statusData == null)
            return StatusCode(500, new { status = "error", message = "Erreur lors de la récupération du statut" });

        if (statusData.Status == "completed")
        {
            var modelName = statusData.ModelName;

            if (!string.IsNullOrWhiteSpace(modelName))
            {
                var existingModel = await _context.ModelAIs.FirstOrDefaultAsync(m => m.Name == modelName);
                if (existingModel == null)
                {
                    var newModel = new ModelAI
                    {
                        Id = Guid.NewGuid(),
                        Name = modelName,
                        IsActive = false
                    };

                    _context.ModelAIs.Add(newModel);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Nouveau modèle ajouté : {modelName}");
                }
            }
            else
            {
                _logger.LogWarning("Nom du modèle non fourni dans la réponse pour sauvegarde.");
            }
        }

        return Ok(statusData);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération du statut de téléchargement");
        return StatusCode(500, new { status = "error", message = "Erreur interne lors de la récupération du statut" });
    }
}


        // GET api/modelai
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelAI>>> GetAll()
        {
            var models = await _context.ModelAIs.ToListAsync();
            return Ok(models);
        }

        // GET api/modelai/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ModelAI>> GetById(Guid id) 
        {
            var model = await _context.ModelAIs.FindAsync(id);
            if (model == null)
                return NotFound(new { message = "Modèle non trouvé" });

            return Ok(model);
        }

        // POST api/modelai/set-active/{id}
        [HttpPost("set-active/{id}")]
        public async Task<IActionResult> SetActiveModel(Guid id) 
        {
            var model = await _context.ModelAIs.FindAsync(id);
            if (model == null)
                return NotFound(new { message = "Modèle non trouvé" });

            // Désactive tous les modèles actifs
            var allModels = await _context.ModelAIs.Where(m => m.IsActive).ToListAsync();
            foreach (var m in allModels)
            {
                m.IsActive = false;
            }

            // Active le modèle demandé
            model.IsActive = true;
            var response = await _httpClient.PostAsync($"/models/set/{model.Name}", null);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Le modèle '{model.Name}' est maintenant actif." });
        }
    }
}
