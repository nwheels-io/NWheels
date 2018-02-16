using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityBilling.Domain.Basics
{
    public struct PersonNameValueObject
    {
        public PersonNameValueObject(string title, string firstName, string lastName, string middleName)
        {
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Title { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string MiddleName { get; }
    }
}
