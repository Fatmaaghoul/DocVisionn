using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Docvision.Models

{
    public class Description
    {
        public Guid Id { get; set; }
        public string? text { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }


        public ICollection<DescriptionObject>? DescriptionObjects { get; set; }
        public string? ModelUsed { get; set; } // model
        public Guid ModelAIId { get; set; }
        [ForeignKey("ModelAIId")]
        [JsonIgnore]
        public ModelAI? modelAI { get; set; }



    }
}

