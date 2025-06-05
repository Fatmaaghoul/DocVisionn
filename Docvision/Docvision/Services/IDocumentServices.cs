using Docvision.Models;

namespace Docvision.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> GetAllDocumentsAsync();
        Task<Document?> GetDocumentByIdAsync(Guid id);
        Task<Document> AddDocumentAsync(IFormFile file, string name, string description, string userId,string AdminId);
        Task<Document?> UpdateDocumentAsync(Guid id, string? name, string? description, string userId);
        Task<bool> DeleteDocumentAsync(Guid id);
        Task<ApplicationUser?> GetUserByDocumentIdAsync(Guid documentId);
    }
}
