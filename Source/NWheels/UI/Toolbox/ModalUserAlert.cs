using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class ModalUserAlert : WidgetBase<ModalUserAlert, Empty.Data, Empty.State>
    {
        public ModalUserAlert(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of WidgetUidlNode

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Enum.GetNames(typeof(UserAlertType)));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<ModalUserAlert, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
