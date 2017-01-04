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
            this.Pages = new List<WidgetUidlNode>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GoNext<TPgWidget, TPgData, TPgState, TPgInput>(
            PresenterBuilder<TPgWidget, TPgData, TPgState>.BehaviorBuilder<TPgInput> behavior)
            where TPgWidget : ControlledUidlNode
            where TPgData : class
            where TPgState : class
        {
            this.DescribingPresenter += (presenter) => {
                behavior.Broadcast(this.Next).WithPayload(vm => vm.Input).TunnelDown();
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GoBack<TPgWidget, TPgData, TPgState, TPgInput>(
            PresenterBuilder<TPgWidget, TPgData, TPgState>.BehaviorBuilder<TPgInput> behavior)
            where TPgWidget : ControlledUidlNode
            where TPgData : class
            where TPgState : class
        {
            behavior.Broadcast(this.Back).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<WidgetUidlNode> Pages { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<object> Next { get; set; }
        public UidlNotification Back { get; set; }

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
            return base.GetNestedWidgets();//.Concat(Pages);
        }
    }
}
