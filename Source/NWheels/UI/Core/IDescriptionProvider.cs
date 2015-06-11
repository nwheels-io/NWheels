using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public interface IDescriptionProvider<TDescription>
    {
        TDescription GetDescription();
    }
}
