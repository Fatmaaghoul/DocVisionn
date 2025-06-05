using Docvision.Models;

namespace Docvision.Dtos
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime UploadDate { get; set; }
        public DateTime? ModifiedAt { get; set; } = null;
        public string? Description { get; set; }
        public string? resumer { get; set; }
        public bool IsAnalysed { get; set; }
        public bool IsExtracted { get; set; }
        public string FileUrl { get; set; } = "";
        public string? Text { get; set; }
        public string? propriétaireId { get; set; } = null;
        public List<string>? AllObjects { get; set; } = new();
        public List<DocumentImageDto> Images { get; set; } = new();
    }

    public class DocumentImageDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string? Description { get; set; }
        public string? Objects { get; set; }
    }
}

