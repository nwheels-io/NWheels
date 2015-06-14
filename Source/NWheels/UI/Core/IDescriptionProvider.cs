using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public interface IDescriptionProvider
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IDescriptionProvider<out TDescription> : IDescriptionProvider
    {
        TDescription GetDescription();
    }
}
