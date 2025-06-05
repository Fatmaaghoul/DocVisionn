using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Docvision.Models;
using Docvision.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Docvision.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DocContext _context;
        private readonly Cloudinary _cloudinary;

        public DocumentService(DocContext context,Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents.ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<Document> AddDocumentAsync(IFormFile file,string name, string description, string userId, string AdminId)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "documents",
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                Name = name,
                UploadDate = DateTime.UtcNow,
                FileUrl = uploadResult.SecureUrl.ToString(),
                UserId = userId,
                description = description,
                propriétaireId = AdminId,
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public async Task<Document?> UpdateDocumentAsync(Guid id, string? name, string? description,string userId)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return null;

            if (!string.IsNullOrEmpty(doc.Name))
            {
                doc.Name = name;
            }

            if (!string.IsNullOrEmpty(doc.description))
            {
                doc.description = description;
            }
            var user = _context.Users.Find(userId);
            if (!string.IsNullOrEmpty(doc.UserId))
            {
                doc.UserId = user.Id;
            }
            doc.ModifiedAt = DateTime.Now;
            _context.Documents.Update(doc);
            await _context.SaveChangesAsync();

            return doc;
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            var doc = await _context.Documents.Include(d => d.Images).FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return false;

            // Supprimer les images associées
            _context.Images.RemoveRange(doc.Images);

            // Supprimer le document
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
    }

        public async Task<ApplicationUser?> GetUserByDocumentIdAsync(Guid documentId)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == document.UserId);
        }
    }
}
