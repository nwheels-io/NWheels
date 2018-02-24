using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Billing
{
    public struct BillingPeriodValueObject
    {
        [MemberContract.Semantics.StartRange(EndMember = nameof(_endDate))]
        [MemberContract.Semantics.DateTimeResolution(DateTimeResolution.Days)]
        [MemberContract.Semantics.Utc]
        private readonly DateTime _startDate;

        [MemberContract.Semantics.DateTimeResolution(DateTimeResolution.Days)]
        [MemberContract.Semantics.Utc]
        private readonly DateTime _endDate;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BillingPeriodValueObject(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime StartDate => _startDate;
        public DateTime EndDate => _endDate;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TimeSpan TimeSpan => _endDate - _startDate;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BillingPeriodValueObject CreateMonthly(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return new BillingPeriodValueObject(startDate, endDate);
        }
    }
}
