using NWheels.Entities;

namespace NWheels.Modules.Geo
{
    [EntityPartContract]
    public interface IEntityPartGeoCoordinate
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}
