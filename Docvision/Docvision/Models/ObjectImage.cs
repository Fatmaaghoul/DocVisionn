using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Docvision.Models
{
    public class ObjectImage
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = "";
        public int OccurenceText { get; set; } = 0;
        public int OccurenceImage { get; set; } = 0;
        public float? Pourcentage { get; set; } = 0;

        public Guid ImageId { get; set; }
        [ForeignKey("ImageId")]
        [JsonIgnore]
        public DocumentImage? Image { get; set; }

        public ICollection<DescriptionObject>? DescriptionObjects { get; set; }


    }
}
