namespace Frock_backend.transport_Company.Interfaces.REST.Resources
{
    public record CompanyResource(
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
