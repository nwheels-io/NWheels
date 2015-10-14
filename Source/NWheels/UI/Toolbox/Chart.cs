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
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
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

        public UidlNotification RequestingData { get; set; }
        public UidlNotification<ChartData> DataReceived { get; set; }
        public UidlNotification<ChartData> DataReady { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, IChartState> presenter)
        {
            presenter.On(DataReceived)
                .AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Data))
                .Then(b => b.Broadcast<ChartData>(DataReady).WithPayload(m => m.Input).TunnelDown());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IChartState
        {
            ChartData Data { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class ChartData
    {
        [DataMember]
        public List<string> Labels { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<SeriesData> Series { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<SummaryData> Summaries { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class SeriesData
        {
            [DataMember]
            public string Label { get; set; }

            [DataMember]
            public List<decimal> Values { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class SummaryData
        {
            [DataMember]
            public string Label { get; set; }

            [DataMember]
            public string Value { get; set; }
        }
    }
}
