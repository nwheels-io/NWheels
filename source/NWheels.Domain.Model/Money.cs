namespace NWheels.Domain.Model
{
    [ValueObject]
    public class Money
    {
        public decimal Price { get; set; }
        public string Currency { get; set; }
    }
}
