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
        public string AuditName { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TransactionScript<TContext, TInput, TOutput> : ITransactionScript<TContext, TInput, TOutput>
    {
        public virtual TInput InitializeInput(TContext context)
        {
            return default(TInput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual TOutput Preview(TInput input)
        {
            return default(TOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract TOutput Execute(TInput input);
    }
}
