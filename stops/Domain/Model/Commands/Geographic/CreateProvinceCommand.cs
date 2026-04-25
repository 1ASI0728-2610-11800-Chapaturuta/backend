namespace Frock_backend.stops.Domain.Model.Commands.Geographic
{
    public record CreateProvinceCommand(int Id, string Name, int FkIdRegion);
}
