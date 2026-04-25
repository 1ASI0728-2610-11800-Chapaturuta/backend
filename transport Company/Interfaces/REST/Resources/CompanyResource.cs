namespace Frock_backend.transport_Company.Interfaces.REST.Resources
{
    public record CompanyResource(
        int Id, // The unique identifier for the company
        string Name, // The name of the company
        string LogoUrl, // The URL to the company's logo image
        int FkIdUser // This is a foreign key to a User entity, indicating the owner or creator of the company
        );
}
