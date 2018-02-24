using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Basics;

namespace ElectricityBilling.Domain.Payments
{
    public class PaymentProcessingResult
    {
        public PaymentProcessingResult(bool success, MoneyValueObject amountPaid, MoneyValueObject amountReceived, string processorMessage)
        {
            Success = success;
            AmountPaid = amountPaid;
            AmountReceived = amountReceived;
            ProcessorMessage = processorMessage;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Success { get; }
        public MoneyValueObject AmountPaid { get; }
        public MoneyValueObject AmountReceived { get; }
        public string ProcessorMessage { get; }
    }
}
