using Microsoft.AspNetCore.Http;

namespace Frock_backend.transport_Company.Domain.Model.Commands
{
    public record CreateCompanyWithFileCommand(
        string Name,
        IFormFile? LogoFile,
        int FkIdUser
    );
}