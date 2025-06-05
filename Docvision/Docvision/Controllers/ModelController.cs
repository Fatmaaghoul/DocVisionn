using Microsoft.AspNetCore.Mvc;
using Docvision.Models;
using Docvision.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/models")]
    public class ModelController : ControllerBase
    {
        private readonly DocContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ModelController> _logger;
        private readonly IMemoryCache _cache;
        private const string AvailableModelsCacheKey = "AvailableModels";

        public ModelController(
            DocContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<ModelController> logger,
            IMemoryCache cache)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("PythonAPI");
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("available-models")]
        public async Task<ActionResult<IEnumerable<AvailableModel>>> GetAvailableModels()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://pdf-api:8000/available-models");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch models from Python API. Status: {StatusCode}", response.StatusCode);
                    return StatusCode(502, "Failed to get models from Python API");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AvailableModelsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var models = result?.Models.Select(m => new AvailableModel
                {
                    Name = m.Name,
                    Value = m.Value,
                    Type = m.Type,
                    Description = m.Description
                }).ToList() ?? new List<AvailableModel>();

                return Ok(models);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting Python API");
                return StatusCode(503, "Python API unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available models");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("current-model")]
        public async Task<ActionResult<CurrentModelResponse>> GetCurrentModel()
        {
            try
            {
                // D'abord vérifier l'API Python
                var response = await _httpClient.GetAsync("http://pdf-api:8000/current-model");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var pythonResponse = JsonSerializer.Deserialize<CurrentModelResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Ok(pythonResponse);
                }

                // Fallback à la base de données si l'API ne répond pas
                var model = await _context.ModelConfigurations
                    .OrderByDescending(m => m.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (model == null)
                {
                    return Ok(new CurrentModelResponse
                    {
                        ModelName = "LLaVA 7B",
                        ModelValue = "llava:7b",
                        IsDefault = true,
                        Type = "vision",
                        Description = "Modèle vision pour analyse d'images"
                    });
                }

                return Ok(new CurrentModelResponse(model));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting Python API");
                // Continue avec le fallback même en cas d'erreur réseau
                var model = await _context.ModelConfigurations
                    .OrderByDescending(m => m.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (model == null)
                {
                    return Ok(new CurrentModelResponse
                    {
                        ModelName = "LLaVA 7B",
                        ModelValue = "llava:7b",
                        IsDefault = true,
                        Type = "vision",
                        Description = "Modèle vision pour analyse d'images"
                    });
                }

                return Ok(new CurrentModelResponse(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current model");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("update-model")]
        public async Task<ActionResult<CurrentModelResponse>> UpdateModel(
            [FromBody] UpdateModelRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ModelValue))
            {
                return BadRequest("ModelValue is required");
            }

            try
            {
                // Vérifier que le modèle existe dans l'API Python
                var availableResponse = await _httpClient.GetAsync("http://pdf-api:8000/available-models");
                if (!availableResponse.IsSuccessStatusCode)
                {
                    return StatusCode(502, "Impossible de vérifier les modèles disponibles");
                }

                var availableContent = await availableResponse.Content.ReadAsStringAsync();
                var availableModels = JsonSerializer.Deserialize<AvailableModelsResponse>(availableContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })?.Models;

                var selectedModel = availableModels?.FirstOrDefault(m => m.Value == request.ModelValue);
                if (selectedModel == null)
                {
                    return BadRequest("Modèle non supporté");
                }

                // Mettre à jour l'API Python
                var updateResponse = await _httpClient.PostAsJsonAsync("http://pdf-api:8000/update-model", request);
                if (!updateResponse.IsSuccessStatusCode)
                {
                    return StatusCode(502, "Échec de la mise à jour du modèle dans l'API Python");
                }

                // Mettre à jour la base de données locale
                var config = new ModelConfiguration
                {
                    ModelName = selectedModel.Name,
                    ModelValue = selectedModel.Value,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ModelConfigurations.Add(config);
                await _context.SaveChangesAsync();

                return Ok(new CurrentModelResponse(config)
                {
                    Type = selectedModel.Type,
                    Description = selectedModel.Description
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting Python API");
                return StatusCode(503, "Python API unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ollama-models")]
        public async Task<ActionResult> GetOllamaModels()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://pdf-api:8000/ollama-models");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(502, "Failed to get Ollama models from Python API");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting Python API");
                return StatusCode(503, "Python API unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Ollama models");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    // DTO Classes
    public class UpdateModelRequest
    {
        public string ModelValue { get; set; }
    }

    public class AvailableModelsResponse
    {
        public List<AvailableModel> Models { get; set; }
        public string Default { get; set; }
    }

    public class AvailableModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class CurrentModelResponse
    {
        public string ModelName { get; set; }
        public string ModelValue { get; set; }
        public bool IsDefault { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public CurrentModelResponse() { }

        public CurrentModelResponse(ModelConfiguration config)
        {
            ModelName = config.ModelName;
            ModelValue = config.ModelValue;
            IsDefault = false;
            // Vous devrez peut-être récupérer ces informations depuis la configuration
            Type = "vision"; // Valeur par défaut
            Description = "Modèle vision pour analyse d'images"; // Valeur par défaut
        }
    }
}