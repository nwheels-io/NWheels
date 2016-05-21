using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class TabbedScreenPartSet : WidgetBase<TabbedScreenPartSet, Empty.Data, Empty.State>
    {
        public TabbedScreenPartSet(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Tabs = new List<UidlScreenPart>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(UidlScreenPart content)
        {
            this.Tabs.Add(content);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public bool DynamicMode { get; set; }
        [DataMember]
        public List<UidlScreenPart> Tabs { get; set; }
        [DataMember]
        public ScreenPartContainer Container { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TabbedScreenPartSet, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
