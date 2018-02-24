using System;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Payments;
using NWheels;

namespace ElectricityBilling.PaymentService.Integration.PayPal
{
    public class PaypalPaymentMethod : PaymentMethodEntity
    {
        [MemberContract.Semantics.EmailAddress]
        private string _paypalEmail;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PaypalPaymentMethod(Injector injector, string title, string paypalEmail) 
            : base(injector, title)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task<PaymentProcessingResult> ProcessPaymentAsync()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string PaypalEmail
        {
            get => _paypalEmail;
            set => _paypalEmail = value;
        }
    }
}
