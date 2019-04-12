using NWheels.Composition.Model;
using NWheels.DevOps.Model;

namespace Demo.TrueTix.BackEnd
{
    public class SeatingService : Microservice<EmptyConfiguration>
    {
        public SeatingService() 
            : base(
                name: "Seating", 
                config: new EmptyConfiguration(), 
                options: new MicroserviceOptions {
                    State = StateOption.Stateless,
                    Availability = AvailabilityOption.High2Nines
                })
        {
        }
        
        
    }
}