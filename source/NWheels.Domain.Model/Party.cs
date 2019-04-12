using System.Collections.Generic;

namespace NWheels.Domain.Model
{
    [Entity]
    public abstract class Party
    {
        public int Id { get; set; }

        public  List<ContactMethod> ContactMethods { get; set; }
        public abstract string DisplayText { get; }
    }

    [Entity]
    public class Person : Party
    {
        public PersonName Name { get; set; }
        public string Avatar { get; set; }
        public override string DisplayText => $"{Name}";
    }

    [Entity]
    public class Organization : Party
    {
        public string Name { get; set; }
        public override string DisplayText => $"{Name}";
    }
}