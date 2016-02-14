using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek weekStartDay)
        {
            int diff = dt.DayOfWeek - weekStartDay;
            
            if ( diff < 0 )
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DateTime StartOfQuarter(this DateTime dt)
        {
            int quarterNumber = ((dt.Month - 1) / 3) + 1;
            return new DateTime(dt.Year, (quarterNumber - 1) * 3 + 1, 1, 0, 0, 0, dt.Kind);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static DateTime StartOfyear(this DateTime dt)
        {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind);
        }
    }
}
