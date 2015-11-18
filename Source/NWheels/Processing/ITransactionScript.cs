using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface ITransactionScript
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransactionScript<TContext, TInput, TOutput> : ITransactionScript
    {
        TInput InitializeInput(TContext context);
        TOutput Preview(TInput input);
        TOutput Execute(TInput input);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TransactionScriptAttribute : Attribute
    {
        public bool SupportsInitializeInput { get; set; }
        public bool SupportsPreview { get; set; }
    }
}
