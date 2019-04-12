using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.TrueTix.Domain;
using NWheels.RestApi.Model;

namespace Demo.TrueTix.Api
{
    public interface ISeatingApi
    {
        Task<PerformanceSeatingPlan> GetPerformanceSeating(int performanceId, int? seatId = null);
    }
}

