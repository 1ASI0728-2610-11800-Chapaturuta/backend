namespace Frock_backend.Discovery.Domain.Model.Queries;

public record GetPopularRoutesQuery(int FkIdUser, int Limit = 10);
