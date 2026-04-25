using Frock_backend.stops.Domain.Model.Commands.Geographic;

namespace Frock_backend.stops.Domain.Model.Aggregates.Geographic
{
    public class Province
    {
        public int Id { get; }
        public string Name { get; set; }
        public int FkIdRegion { get; set; }

        protected Province()
        { 
            Id =0;
            Name = string.Empty;
            FkIdRegion = 0;
        }

        public Province(CreateProvinceCommand command)
        {
            Id = command.Id;
            Name = command.Name;
            FkIdRegion = command.FkIdRegion;
        }
    }
}
