namespace NWheels.Domain.Model
{
    [ValueObject]
    public class PersonName
    {
        public string First { get; set; } 
        public string Last { get; set; } 
        public string Middle { get; set; } 
        public string Salutation { get; set; }

        public string FullName => $"{First} {Last}";
    }
}
