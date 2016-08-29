using System;

namespace NWheels.Api
{
    public struct YearMonth
    {
        private DateTime _value;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public YearMonth(int year, int month)
        {
            _value = new DateTime(
                year,
                month, 
                day: 1,
                hour: 0,
                minute: 0, 
                second: 0, 
                kind: DateTimeKind.Utc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public YearMonth AddMonths(int months)
        {
            return new YearMonth(_value.AddMonths(months));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public YearMonth(DateTime dateTime) 
            : this(dateTime.Year, dateTime.Month)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Year
        {
            get
            {
                return _value.Year;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Month
        {
            get
            {
                return _value.Month;
            }
        }
    }
}