using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class TaskPad : WidgetBase<TaskPad, Empty.Data, Empty.State>
    {
        public TaskPad(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Items = new List<TaskPadItem>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetBase<TaskPad,Data,State>

        protected override void DescribePresenter(PresenterBuilder<TaskPad, Empty.Data, Empty.State> presenter)
        {
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<TaskPadItem> Items { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class TaskPadItem : WidgetBase<TaskPadItem, Empty.Data, Empty.State>
    {
        public TaskPadItem(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetBase<TaskPadItem,Data,State>

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(new[] { Text, Description });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TaskPadItem, Empty.Data, Empty.State> presenter)
        {
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Description { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand Perform { get; set; }
    }
}
