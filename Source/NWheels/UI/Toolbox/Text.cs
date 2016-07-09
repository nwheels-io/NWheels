using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Globalization.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Text : WidgetBase<Text, Empty.Data, Text.IState>
    {
        public Text(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            Parameters = new Dictionary<string, object>();
            TextSize = TextSize.Normal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return base.GetTranslatables().Concat(LocaleEntryKey.Enumerate(
                this,
                this.Contents, null));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Contents { get; set; }
        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }
        [DataMember]
        public TextSize TextSize { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<string> FormatSetter { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Text, Empty.Data, Text.IState> presenter)
        {
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IState
        {
            string Contents { get; set; }
            Dictionary<string, object> Parameters { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum TextSize
    {
        Small,
        Normal,
        Header,
        LargeHeader,
        HugeHeader
    }
}
