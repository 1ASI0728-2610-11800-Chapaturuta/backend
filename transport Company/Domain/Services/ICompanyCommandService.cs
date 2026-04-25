using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.transport_Company.Domain.Model.Commands;

namespace Frock_backend.transport_Company.Domain.Services
{
    public interface ICompanyCommandService
    {
        Task<Company?> Handle(CreateCompanyCommand command);
        Task<Company?> Handle(UpdateCompanyCommand command);
        Task<Company?> Handle(DeleteCompanyCommand command);
    }
}
