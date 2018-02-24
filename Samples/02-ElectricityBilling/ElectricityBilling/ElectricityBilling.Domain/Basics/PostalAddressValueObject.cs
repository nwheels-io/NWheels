using System;
using System.Collections.Generic;
using System.Text;
using NWheels;
using NWheels.Ddd;
using NWheels.I18n;

namespace ElectricityBilling.Domain.Basics
{
    public class PostalAddressValueObject
    {
        [NWheels.MemberContract.Required]
        [NWheels.MemberContract.Semantics.Street]
        private readonly string _street;

        [NWheels.MemberContract.Required]
        [NWheels.MemberContract.Semantics.StreetNumber]
        private readonly string _number;

        [NWheels.MemberContract.Required]
        [NWheels.MemberContract.Semantics.City]
        private readonly string _city;

        [NWheels.MemberContract.Semantics.CountryState]
        private readonly string _state;

        [NWheels.MemberContract.Required]
        [NWheels.MemberContract.Semantics.Country]
        private readonly string _country;

        [NWheels.MemberContract.Required]
        [NWheels.MemberContract.Semantics.ZipCode]
        private readonly string _zip;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PostalAddressValueObject(string street, string number, string city, string state, string country, string zip)
        {
            _street = street;
            _number = number;
            _city = city;
            _state = state;
            _country = country;
            _zip = zip;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Street => _street;
        public string Number => _number;
        public string City => _city;
        public string State => _state;
        public string Country => _country;
        public string Zip => _zip;
    }
}
