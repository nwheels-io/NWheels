using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.TrueTix.Domain;
using NWheels.RestApi.Model;

namespace Demo.TrueTix.Api
{
    public interface ISeatingApi
    {
        Task<PerformanceSeatingPlan> GetSeatingMap(int performanceId);
        Task<PerformanceSeat> GetSeat(int performanceId, int seatId);
    }
}

