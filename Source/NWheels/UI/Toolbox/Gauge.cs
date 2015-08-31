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
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Gauge : WidgetBase<Gauge, Empty.Data, Empty.State>
    {
        public Gauge(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.Values = new List<GaugeValue>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables()
                .ConcatIf(BadgeText)
                .Concat(Values.SelectMany(v => v.GetTranslatables()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string BadgeText { get; set; }
        [DataMember]
        public List<GaugeValue> Values { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class GaugeValue
        {
            public IEnumerable<string> GetTranslatables()
            {
                if ( !string.IsNullOrEmpty(Label) )
                {
                    yield return Label;
                }
                
                if ( !string.IsNullOrEmpty(ChangeLabel) )
                {
                    yield return ChangeLabel;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public decimal Value { get; set; }
            [DataMember]
            public string Label { get; set; }
            [DataMember]
            public decimal OldValue { get; set; }
            [DataMember]
            public GaugeChangeType ChangeType { get; set; }
            [DataMember]
            public string ChangeLabel { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum GaugeChangeType
    {
        None,
        Absolute,
        Percentage,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class GaugeExtensions
    {
        public static Gauge Badge(this Gauge gauge, string value)
        {
            gauge.BadgeText = value;
            return gauge;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Gauge Value(this Gauge gauge, string label, GaugeChangeType changeType = GaugeChangeType.None, string changeLabel = null)
        {
            gauge.Values.Add(new Gauge.GaugeValue {
                Label = label,
                ChangeType = changeType,
                ChangeLabel = changeLabel
            });
            
            return gauge;
        }
    }
}
