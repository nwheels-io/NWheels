using System.Threading.Tasks;

namespace ElectricityBilling.Domain.Sensors
{
    public interface ISensorHubService
    {
        Task PostSensorStatusChangeAsync(string sensorId, bool active);
    }
}