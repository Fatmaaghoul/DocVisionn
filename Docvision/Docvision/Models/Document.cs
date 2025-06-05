using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Docvision.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime UploadDate { get; set; }
        public DateTime? ModifiedAt { get; set; } 
        public string? description { get; set; } = "";
        public bool isAnalysed { get; set; }=false;
        public bool isExtracted { get; set; }=false ;
        public string? resumer { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string? Text { get; set; }
        public List<string>? AllObjects { get; set; } = [];
        public ICollection<DocumentImage>? Images { get; set; }

        public string UserId { get; set; } 

        [ForeignKey("UserId")]
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public string? propriétaireId { get; set; }
    }
}
