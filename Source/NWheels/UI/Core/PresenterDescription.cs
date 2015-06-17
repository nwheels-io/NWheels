using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public class PresenterDescription : UINodeDescription
    {
        public PresenterDescription(UIContentElementDescription parent)
            : base("Presenter", parent)
        {
            base.NodeType = UINodeType.Presenter;
            this.DataBindings = new List<DataBindingDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<DataBindingDescription> DataBindings { get; private set; }
    }
}
