namespace Frock_backend.stops.Domain.Model.Commands.Geographic
{
    /// <summary>
    /// Comando para crear una nueva región geográfica.
    /// </summary>
    /// <param name="Id">Identificador único de la región (como código reg-1)</param>
    /// <param name="Name">Nombre oficial de la región</param>
    public record CreateRegionCommand(int Id, string Name);
}
