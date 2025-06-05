using Docvision.Models;
using Docvision.Persistance;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Docvision.Controllers
{
    public class DescriptionController : Controller
    {
        private readonly DocContext _docContext;
    
        public DescriptionController(DocContext docContext)
        {
            _docContext = docContext;
       
        }
        [HttpGet("api/description/by-object/{objectId}")]
        public async Task<IActionResult> GetDescriptionsByObjectId(Guid objectId)
        {
            try
            {
                var descriptions = await _docContext.DescriptionObjects
                    .Where(d => d.ObjectId == objectId)
                    .Include(d => d.Description) // inclure la navigation vers Description
                    .Select(d => new
                    {
                        d.DescriptionId,
                        d.Description!.text,
                        d.Description.Created,
                        d.Description.Updated
                    })
                    .ToListAsync();

                if (!descriptions.Any())
                {
                    return NotFound($"Aucune description trouvée pour l'objet avec l'ID {objectId}.");
                }

                return Ok(descriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }


        


[HttpDelete("api/descriptions/{descriptionId}")]
        public async Task<IActionResult> DeleteDescription(Guid descriptionId)
        {
            try
            {
                // Check if the description exists
                var description = await _docContext.Descriptions.FindAsync(descriptionId);

                if (description == null)
                {
                    return NotFound($"Description avec ID {descriptionId} non trouvée.");
                }

                // Remove associated DescriptionObjects (many-to-many relationship)
                if (description.DescriptionObjects != null && description.DescriptionObjects.Any())
                {
                    _docContext.DescriptionObjects.RemoveRange(description.DescriptionObjects);
                }

                // Remove the description
                _docContext.Descriptions.Remove(description);
                await _docContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Description supprimée avec succès."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
        }
    }
}

