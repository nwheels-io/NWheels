using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IRootContentContainer<out TInputParam> : INavigationTarget<TInputParam>
    {
        IWidget ContentRoot { get; set; }
    }
}
