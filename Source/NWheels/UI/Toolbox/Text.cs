using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Text : WidgetBase<Text, Empty.Data, Empty.State>
    {
        public Text(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Parameters = new Dictionary<string, object>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().ConcatIf<string>(this.Format);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Format { get; set; }
        [DataMember]
        public Dictionary<string, object> Parameters { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Text, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
