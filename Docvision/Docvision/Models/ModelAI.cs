namespace Docvision.Models
{
    public class ModelAI
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
       
        public ICollection<Description>? descriptions { get; set; }
    }
}
