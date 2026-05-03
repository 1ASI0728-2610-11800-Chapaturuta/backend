namespace Frock_backend.transport_Company.Domain.Model.Commands
{
    public record UpdateCompanyCommand(
        int Id,
        string Name,
        string LogoUrl,
        int FkIdUser,
        string? Ruc,
        string? Phone,
        string? Email,
        string? Address,
        string? Description
        );
}
