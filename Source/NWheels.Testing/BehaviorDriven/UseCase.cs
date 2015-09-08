using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Testing.BehaviorDriven
{
    public abstract class UseCase : ExecutableSpecification
    {
        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("Use case [ {0} ]", this.GetType().Name.TrimTail("UseCase").SplitPascalCase());
        }

        #endregion
    }
}
