using Microsoft.AspNetCore.Http;

namespace Frock_backend.shared.Domain.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "companies");
        Task<bool> DeleteImageAsync(string publicId);
    }
}