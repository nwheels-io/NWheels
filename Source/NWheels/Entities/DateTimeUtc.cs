using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public class DateTimeUtc
    {
        private readonly DateTime _dt;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DateTimeUtc(DateTime dt)
        {
            _dt = dt;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator DateTimeOffset(DateTimeUtc p)
        {
            return DateTime.SpecifyKind(p._dt, DateTimeKind.Utc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator DateTimeUtc(DateTimeOffset dto)
        {
            return new DateTimeUtc(dto.DateTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator DateTime(DateTimeUtc dtr)
        {
            return dtr._dt;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator DateTimeUtc(DateTime dt)
        {
            return new DateTimeUtc(dt);
        }
    }
}
