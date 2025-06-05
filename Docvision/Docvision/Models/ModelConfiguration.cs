// ModelConfiguration.cs
using System;

namespace Docvision.Models
{
    public class ModelConfiguration
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
        public string ModelValue { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}