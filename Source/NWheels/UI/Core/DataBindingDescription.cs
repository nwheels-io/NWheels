using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public class DataBindingDescription : UINodeDescription
    {
        public DataBindingDescription(string idName, PresenterDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.DataBinding;
        }
    }
}
