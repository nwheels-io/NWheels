using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.TypeModel;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Chart : WidgetBase<Chart, Empty.Data, Empty.State>
    {
        public Chart(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BindToModelSetter(UidlNotification<ChartData> modelSetter)
        {
            this.BindToModelSetter(modelSetter, dataProperty: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BindToModelSetter<TModel>(UidlNotification<TModel> modelSetter, Expression<Func<TModel, ChartData>> dataProperty)
        {
            ModelSetterQualifiedName = modelSetter.QualifiedName;
            DataExpression = (
                dataProperty != null ? 
                dataProperty.ToNormalizedNavigationString("input").TrimLead("input.") : 
                null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string DataExpression { get; set; }
        [DataMember]
        public string ModelSetterQualifiedName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<string> ModeChanged { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, Empty.State> presenter)
        {
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
        public List<AbstractSeriesData> Series { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<SummaryData> Summaries { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class SummaryData
        {
            [DataMember]
            public string Label { get; set; }

            [DataMember]
            public string Value { get; set; }

            [DataMember]
            public int? Percent { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public abstract class AbstractSeriesData
        {
            [DataMember]
            public string Label { get; set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class CategoricalSeriesData : AbstractSeriesData
        {
            [DataMember]
            public ChartSeriesType Type { get; set; }

            [DataMember]
            public List<decimal> Values { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class TimestampSeriesData : AbstractSeriesData
        {
            [DataMember]
            public List<TimestampPoint> Points { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class TimestampPoint : AbstractSeriesData
        {
            [DataMember]
            public DateTime UtcTimestamp { get; set; }
            [DataMember]
            public decimal Value { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ChartSeriesType
    {
        Point = 10,
        Line = 20,
        Bar = 30,
        StackedBar = 40
    }
}
