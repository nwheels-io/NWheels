using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel
{
    [Flags]
    public enum TimeUnits
    {
        None = 0,
        Millisecond = 0x01,
        Second = 0x02,
        Minute = 0x04,
        Hour = 0x08,
        Day = 0x10,
        Week = 0x20,
        Month = 0x40,
        Year = 0x80,
        HourMinuteSecond = Hour | Minute | Second,
        HourMinute = Hour | Minute,
        MinuteSecond = Minute | Second
    }
}
