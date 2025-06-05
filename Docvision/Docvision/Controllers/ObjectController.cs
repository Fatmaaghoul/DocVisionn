using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Docvision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docvision.Persistance;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly DocContext _context;

        public ObjectController(DocContext context)
        {
            _context = context;
        }

        [HttpGet("by-document/{docId}")]
        public async Task<ActionResult<IEnumerable<ObjectImage>>> GetObjectsByDocumentId(Guid docId)
        {
            try
            {
                var objects = await _context.Objects
                    .Where(o => o.Image.DocumentId == docId )
                    //&& o.Pourcentage > 0
                    .Include(o => o.Image)
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.OccurenceText,
                        o.OccurenceImage,
                        o.Pourcentage,
                        o.ImageId
                    })
                    .ToListAsync();

                if (!objects.Any())
                {
                    return NotFound(new { message = "No objects found for the specified document with percentage > 0" });
                }

                return Ok(objects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving objects", error = ex.Message });
            }
        }
    }
}