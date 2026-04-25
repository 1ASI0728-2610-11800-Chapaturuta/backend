using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Frock_backend.stops.Interfaces.REST.Resources
{
    public class CreateStopFormResource
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? GoogleMapsUrl { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        
        [Required]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public int FkIdCompany { get; set; }
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        public string Reference { get; set; } = string.Empty;

        [Required]
        public int FkIdDistrict { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}