using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityBilling.Domain.Basics
{
    public struct PersonNameValueObject
    {
        private readonly string _title;
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly string _middleName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PersonNameValueObject(string title, string firstName, string lastName, string middleName)
        {
            _title = title;
            _firstName = firstName;
            _lastName = lastName;
            _middleName = middleName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Title => _title;
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public string MiddleName => _middleName;
    }
}
