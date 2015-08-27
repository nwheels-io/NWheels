using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public interface ITransactionScript
    {
        Type InputType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface ITransactionScript<in TInput> : ITransactionScript
    {
        void Execute(TInput input);
    }
}
