using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IWidget : IUIElement
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICompositeWidget : IWidget, IEnumerable<IWidget>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface ITemplateWidget : ICompositeWidget
    {
    }
}
