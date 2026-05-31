using Frock_backend.stops.Domain.Model.Commands;

namespace Frock_backend.stops.Domain.Model.Aggregates
{
    public class Stop
    {
        public int Id { get; }
        public string Name { get;  set; }
        public string? GoogleMapsUrl { get;  set; }
        public string? ImageUrl { get;  set; }
        public int FkIdDriver { get;  set; }
        public string Address { get;  set; }
        public string Reference { get;  set; }
        public int FkIdDistrict { get;  set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        protected Stop()
        {
            Name = string.Empty;
            GoogleMapsUrl = string.Empty;
            ImageUrl = string.Empty;
            FkIdDriver = 0;
            Address = string.Empty;
            Reference = string.Empty;
            FkIdDistrict = 0;
            Latitude = null;
            Longitude = null;
        }
        public Stop(int id, string name, string address, int fk_id_driver, int fk_id_district)
        {
            this.Id = id;
            this.Name = name;
            this.Address = address;
            this.FkIdDriver = fk_id_driver;
            this.FkIdDistrict = fk_id_district;
        }
        public Stop(CreateStopCommand command)
        {
            Name = command.Name;
            GoogleMapsUrl = command.GoogleMapsUrl;
            ImageUrl = command.ImageUrl;
            FkIdDriver = command.FkIdDriver;
            Address = command.Address;
            Reference = command.Reference;
            FkIdDistrict = command.FkIdDistrict;
            Latitude = command.Latitude;
            Longitude = command.Longitude;
        }

        public Stop(UpdateStopCommand command)
        {
            Id = command.Id;
            Name = command.Name;
            GoogleMapsUrl = command.GoogleMapsUrl;
            ImageUrl = command.ImageUrl;
            FkIdDriver = command.FkIdDriver;
            Address = command.Address;
            Reference = command.Reference;
            FkIdDistrict = command.FkIdDistrict;
            Latitude = command.Latitude;
            Longitude = command.Longitude;
        }

        public Stop(DeleteStopCommand command)
        {
            Id = command.Id;
            Name = "";
            GoogleMapsUrl = "";
            ImageUrl = "";
            FkIdDriver = 0;
            Address = "";
            Reference = "";
            FkIdDistrict = 0;
        }
    }
}
