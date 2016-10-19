using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hapil.Operands;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.Processing.Documents;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Splash : WidgetBase<Splash, Empty.Data, Empty.State>
    {
        public Splash(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            yield return InsideContent;
            yield return UtilityBar;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return base.GetTranslatables().ConcatOne(new LocaleEntryKey(this.PoweredBy, this, "PoweredBy"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Splash, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember, ManuallyAssigned]
        public WidgetUidlNode InsideContent { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Container UtilityBar { get; set; }
        
        [DataMember]
        public string PoweredBy { get; set; }

        [DataMember]
        public FormattedDocument LogoImage { get; set; }
    }
}
