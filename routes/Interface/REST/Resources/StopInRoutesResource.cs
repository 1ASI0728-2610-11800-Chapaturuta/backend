namespace Frock_backend.routes.Interface.REST.Resources
{
    public record StopInRoutesResource
    (
        int id, // The unique identifier for the stop
        string name, // The name of the stop
        string? image_url,
        string address,
        int fk_company_id, // The foreign key to the company that owns the stop
        int fk_district_id // The foreign key to the district that the stop belongs to
    );
}
