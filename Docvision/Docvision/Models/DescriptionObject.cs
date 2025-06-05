using System.ComponentModel.DataAnnotations.Schema;

namespace Docvision.Models
{
    public class DescriptionObject
    {
        public Guid DescriptionId { get; set; }
        [ForeignKey("DescriptionId")]
        public Description? Description { get; set; }

        public Guid ObjectId { get; set; }
        [ForeignKey("ObjectId")]
        public ObjectImage? DetectedObject { get; set; }
    }
}
