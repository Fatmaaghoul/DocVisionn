using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Docvision.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string RefreshToken { get; set; }
        [JsonIgnore]
        public List<Document>? Documents { get; set; } = new();
     //   public List<Folder> Folders { get; set; }= new();
    }

}

