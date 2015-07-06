using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class Crud<TEntity> : WidgetBase<Gauge, Empty.Data, Empty.State>
        where TEntity : class
    {
        public Crud(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "Crud";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, Empty.State> presenter)
        {
        }
    }
}
