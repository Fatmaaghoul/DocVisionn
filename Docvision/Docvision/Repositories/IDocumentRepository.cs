using Docvision.Dtos;
using Docvision.Models;
using Microsoft.AspNetCore.Identity;
using static Back.Controllers.DocumentController;

namespace Docvision.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document> GetDocumentByIdAsync(Guid Id,string userId);
        Task<List<Document>> GetAllDocumentAsync(string userId);
        Task<Document> AddDocumentAsync(IFormFile file,string Name ,string description, string userId);
        Task<Document> UpdateDocumentAsync(Guid Id, DocumentUpdateDto updatedDocument, string userId);

        Task<Document> DeleteDocumentAsync(Guid Id, string userId);

        Task<ExtractedResult> ExtractTextAndImagesAsync(string pdfUrl);




    }
}
