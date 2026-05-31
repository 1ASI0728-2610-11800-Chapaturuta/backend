namespace Frock_backend.Driver.Domain.Model.Commands;

public record SetRouteDurationCommand(int FkIdTariff, int FkIdRoute, int EstimatedMinutes);
