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
    public class Gauge : WidgetBase<Gauge, Empty.Data, Empty.State>
    {
        public Gauge(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Values = new List<GaugeValue>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public string Badge { get; set; }
        [DataMember]
        public List<GaugeValue> Values { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class GaugeValue
        {
            [DataMember]
            public decimal Value { get; set; }
            [DataMember]
            public string Label { get; set; }
            [DataMember]
            public GaugeValueType Type { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum GaugeValueType
    {
        Absolute,
        Percentage,
        AbsoluteChange,
        PercentageChange
    }
}
