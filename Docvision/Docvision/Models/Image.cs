using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Docvision.Models
{
    public class DocumentImage
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
   
        public Guid DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        [JsonIgnore]
        public Document? Document { get; set; }
        public ICollection<ObjectImage>? Objects { get; set; }

    }
}