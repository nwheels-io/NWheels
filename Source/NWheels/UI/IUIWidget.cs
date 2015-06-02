using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUIWidget
    {
        string IdName { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICompositeUIWidget : IUIWidget, IEnumerable<IUIWidget>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface ITemplateUIWidget : ICompositeUIWidget
    {
    }
}
