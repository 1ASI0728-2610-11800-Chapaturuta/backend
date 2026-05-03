using Frock_backend.transport_Company.Domain.Model.Commands;

namespace Frock_backend.transport_Company.Domain.Model.Aggregates
{
    public class Company
    {
        public int Id { get; }
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
        public int FkIdUser { get; set; }
        public string? Ruc { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        protected Company()
        {
            Name = string.Empty;
            LogoUrl = string.Empty;
            FkIdUser = 0;
        }

        public Company(CreateCompanyCommand command)
        {
            Name = command.Name;
            LogoUrl = command.LogoUrl;
            FkIdUser = command.FkIdUser;
        }

        public Company(UpdateCompanyCommand command)
        {
            Id = command.Id;
            Name = command.Name;
            LogoUrl = command.LogoUrl;
            FkIdUser = command.FkIdUser;
            Ruc = command.Ruc;
            Phone = command.Phone;
            Email = command.Email;
            Address = command.Address;
            Description = command.Description;
        }

        public Company(DeleteCompanyCommand command)
        {
            Id = command.Id;
            Name = "";
            LogoUrl = "";
            FkIdUser = 0;
        }
    }
}
