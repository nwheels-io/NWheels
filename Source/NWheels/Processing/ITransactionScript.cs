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
        TOutput Execute(TInput input);
    }
}
