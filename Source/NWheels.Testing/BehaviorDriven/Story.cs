using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Testing.BehaviorDriven
{
    public abstract class Story : ExecutableSpecification
    {
        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("Story [ {0} ]", this.GetType().Name.TrimTail("Story").SplitPascalCase());
        }

        #endregion
    }
}
