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
    public class Chart : WidgetBase<Chart, Empty.Data, Chart.IChartState>
    {
        public Chart(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string CurrentValueDataProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ManuallyAssigned]
        public UidlNotification UpdateSource { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, IChartState> presenter)
        {
            presenter.On(UpdateSource).AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Data));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IChartState
        {
            Empty.Payload Data { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class ChartExtensions
    {
        public static Chart DataSource<TPayload>(
            this Chart chart,
            UidlNotification<TPayload> updateSource,
            Expression<Func<TPayload, int>> valueProperty,
            Expression<Func<TPayload, int>> oldValueProperty = null)
        {
            chart.UpdateSource = updateSource;
            chart.CurrentValueDataProperty = valueProperty.ToNormalizedNavigationString("input").TrimLead("input.");

            return chart;
        }
    }
}
