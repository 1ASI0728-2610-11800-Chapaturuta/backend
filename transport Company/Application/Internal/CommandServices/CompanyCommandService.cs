using Frock_backend.shared.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.transport_Company.Domain.Model.Commands;
using Frock_backend.transport_Company.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Services;

namespace Frock_backend.transport_Company.Application.Internal.CommandServices
{
    public class CompanyCommandService(ICompanyRepository companyRepository, IUnitOfWork unitOfWork) : ICompanyCommandService
    {
        public async Task<Company?> Handle(CreateCompanyCommand command)
        {
            var existingCompany = await companyRepository.FindByNameAsync(command.Name);
            if (existingCompany != null)
            {
                throw new Exception($"Company with name '{command.Name}' already exists.");
            }
            var newCompany = new Company(command);
            try
            {
                await companyRepository.AddAsync(newCompany);
                await unitOfWork.CompleteAsync();
                return newCompany;
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error creating company with name {CompanyName}.", command.Name);
                return null; // Signal failure to the controller
            }
        }
        public async Task<Company?> Handle(UpdateCompanyCommand command)
        {
            var companyToUpdate = await companyRepository.FindByIdAsync(command.Id);
            if (companyToUpdate == null)
            {
                return null; // Company not found
            }

            // Apply changes from the command to the fetched entity
            companyToUpdate.Name = command.Name;
            companyToUpdate.LogoUrl = command.LogoUrl;
            companyToUpdate.FkIdUser = command.FkIdUser;

            try
            {
                companyRepository.Update(companyToUpdate); // Update the fetched and modified entity
                await unitOfWork.CompleteAsync();
                return companyToUpdate; // Return the updated entity
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error updating company with ID {CompanyId}.", command.Id);
                return null; // Signal failure to the controller
            }
        }

        public async Task<Company?> Handle(DeleteCompanyCommand command)
        {
            var companyToDelete = await companyRepository.FindByIdAsync(command.Id);
            if (companyToDelete == null)
            {
                return null; // Company not found
            }
            try
            {
                companyRepository.Remove(companyToDelete); // Delete the fetched entity
                await unitOfWork.CompleteAsync();
                return companyToDelete; // Return the deleted entity
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error deleting company with ID {CompanyId}.", command.Id);
                return null; // Signal failure to the controller
            }
        }

    }
}
