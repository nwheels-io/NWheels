using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Wizard : WidgetBase<Wizard, Empty.Data, Empty.State>
    {
        public Wizard(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<WidgetUidlNode> Pages { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Wizard, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            base.OnBuild(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return base.GetNestedWidgets().Concat(Pages);
        }
    }
}
