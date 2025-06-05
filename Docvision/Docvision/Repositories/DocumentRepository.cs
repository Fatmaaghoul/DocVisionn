using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Docvision.Models;
using Microsoft.EntityFrameworkCore;
using Docvision.Persistance;
using Docvision.Dtos;
using static Back.Controllers.DocumentController;
using System.Net.Http;
using System.Text;
using UglyToad.PdfPig;


namespace Docvision.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly Cloudinary _cloudinary;
        private readonly DocContext _Context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DocContext _context;
        private readonly HttpClient _httpClient;


        public DocumentRepository(Cloudinary cloudinary, DocContext docContext, IHttpClientFactory httpClientFactory, DocContext context , HttpClient httpClient)
        {
            _cloudinary = cloudinary;
            _Context = docContext;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _httpClient = httpClient;

        }

        public async Task<Document> AddDocumentAsync(IFormFile file,string Name, string description, string userId)
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
                Name = Name,
                UploadDate = DateTime.Now,
                FileUrl = uploadResult.SecureUrl.ToString(),
                UserId = userId,
                description = description,
                propriétaireId = userId, 
            };

            _Context.Documents.Add(document);
            await _Context.SaveChangesAsync();

            return document;
        }

        public async Task<Document?> DeleteDocumentAsync(Guid id, string userId)
        {
            try
            {
                // Validate inputs
                if (id == Guid.Empty)
                {
                    throw new ArgumentException("Document ID cannot be empty", nameof(id));
                }
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
                }

                // Retrieve document with user verification and include Images
                var document = await _Context.Documents
                    .Include(d => d.Images) // Ensure Images is loaded
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

                if (document == null)
                {
                    return null;
                }

                // Use transaction for database operations
                using var transaction = await _Context.Database.BeginTransactionAsync();
                try
                {
                    // Delete associated images if they exist
                    if (document.Images != null && document.Images.Any())
                    {
                        _Context.Images.RemoveRange(document.Images);
                    }

                    // Delete document
                    _Context.Documents.Remove(document);
                    await _Context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return document;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id, string userId)
        {
            return await _context.Documents
                .Include(d => d.Images)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        }

        public async Task<List<Document>> GetAllDocumentAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .Include(d => d.Images)
                .ToListAsync();
        }

        public async Task<Document?> UpdateDocumentAsync(Guid id, DocumentUpdateDto updatedDocument, string userId)
        {
            var document = await _Context.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (document == null)
            {
                return null; // Si aucun document n'est trouvé, on retourne null
            }

            // Mise à jour uniquement du nom et de la description, si spécifié
            if (!string.IsNullOrEmpty(updatedDocument.Name))
            {
                document.Name = updatedDocument.Name;
            }

            if (!string.IsNullOrEmpty(updatedDocument.Description))
            {
                document.description = updatedDocument.Description;
            }
            document.ModifiedAt = DateTime.Now;
            _Context.Documents.Update(document);
            await _Context.SaveChangesAsync();

            return document;
        }
        public async Task<ExtractedResult> ExtractTextAndImagesAsync(string pdfUrl)
        {
            var result = new ExtractedResult
            {
                Text = "",
                ImageUrls = new List<string>()
            };

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(pdfUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var pdfDocument = PdfDocument.Open(stream);

            // Extraction du texte
            var textBuilder = new StringBuilder();
            foreach (var page in pdfDocument.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }

            result.Text = textBuilder.ToString(); 
            if (string.IsNullOrWhiteSpace(result.Text))
{
    result.Text = "No text in this file";
}

            var requestBody = new
            {
                text = result.Text
            };
            
            
            var res = await _httpClient.PostAsJsonAsync("http://pdf-api:8000/translate", requestBody);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<TranslateResponse>();

            if (json != null && !string.IsNullOrWhiteSpace(json.translated_text))
            {
                result.Text = json.translated_text;
            }


            // Extraction des images (sans analyse)
            foreach (var page in pdfDocument.GetPages())
            {
                foreach (var image in page.GetImages())
                {
                    var bytes = image.RawBytes.ToArray();
                    if (bytes.Length == 0) continue;

                    var imageUrl = await UploadImageToCloudinary(
                        bytes,
                        $"img_{Guid.NewGuid()}"
                    );

                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        result.ImageUrls.Add(imageUrl);
                    }
                }
            }

            return result;
        }
        private async Task<string> UploadImageToCloudinary(byte[] imageData, string publicId)
        {
            using var stream = new MemoryStream(imageData);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(publicId, stream),
                PublicId = publicId,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString() ?? string.Empty;
        }


    }
    public class TranslateResponse
    {
        public string translated_text { get; set; } = string.Empty;
    }

}
