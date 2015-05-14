using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging.Core;

namespace NWheels.Utilities.Core
{
    public abstract class UtilityToolBase
    {
        public abstract void ExecuteWithOptionsObject(object optionsObject);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class UtilityToolBase<TOptions> : UtilityToolBase
    {
        private readonly IPlainLog _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UtilityToolBase(IPlainLog log)
        {
            _log = log;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ExecuteWithOptionsObject(object optionsObject)
        {
            this.Execute((TOptions)optionsObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void Execute(TOptions options);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected  IPlainLog Log
        {
            get { return _log; }
        }
    }
}
