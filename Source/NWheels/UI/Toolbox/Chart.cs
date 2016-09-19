﻿using System;
using System.Collections;
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
using NWheels.Utilities;

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

        public void BindToVisualizedQueryResults(UidlNotification modelSetter)
        {
            ModelSetterQualifiedName = modelSetter.QualifiedName;
            DataExpression = ExpressionUtility.GetPropertyInfoFrom<ApplicationEntityService.QueryResults>(x => x.Visualization).Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string DataExpression { get; set; }
        [DataMember]
        public string ModelSetterQualifiedName { get; set; }
        [DataMember]
        public WidgetSize Height { get; set; }
        [DataMember]
        public ChartData.SeriesPalette? Palette { get; set; }
        [DataMember]
        public bool LogarithmicScale { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<string> ModeChanged { get; set; }
        public UidlNotification<RangeSelection> RangeSelected { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, Empty.State> presenter)
        {
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RangeSelection
        {
            public object From { get; set; }
            public object To { get; set; }
            public object SeriesIndex { get; set; }
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
            [DataMember]
            public ChartSeriesType Type { get; set; }
            [DataMember]
            public abstract XAxisMode Mode { get; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class CategoricalSeriesData : AbstractSeriesData
        {
            [DataMember]
            public List<decimal> Values { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public override XAxisMode Mode
            {
                get { return XAxisMode.Category; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class TimeSeriesData : AbstractSeriesData
        {
            public TimeSeriesData StepSize(TimeSpan value)
            {
                this.MillisecondStepSize = (int)value.TotalMilliseconds;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public List<TimeSeriesPoint> Points { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public long MillisecondStepSize { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public override XAxisMode Mode
            {
                get { return XAxisMode.Time; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class TimeSeriesPoint
        {
            [DataMember]
            public DateTime UtcTimestamp { get; set; }
            [DataMember]
            public decimal Value { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public long UnixUtcTimestamp 
            {
                get
                {
                    TimeSpan baseDate = new System.TimeSpan(new DateTime(1970, 1, 1).Ticks);
                    DateTime timestamp = UtcTimestamp.Subtract(baseDate);
                    return (long)(timestamp.Ticks / 10000);                            
                } 
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum SeriesPalette
        {
            DataSeries,
            LogLevelSeries,
            PassFailSeries
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IVisualizedQueryable : IQueryable
    {
        ChartData Visualization { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class VisualizedQueryable<TResultRow> : IQueryable<TResultRow>, IVisualizedQueryable
    {
        private readonly IQueryable<TResultRow> _queryable;
        private readonly ChartData _visualization;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public VisualizedQueryable(IQueryable<TResultRow> queryable, ChartData visualization)
        {
            _queryable = queryable;
            _visualization = visualization;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<TResultRow> IEnumerable<TResultRow>.GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IQueryable

        Expression IQueryable.Expression
        {
            get { return _queryable.Expression; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IQueryable.ElementType
        {
            get { return _queryable.ElementType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get { return _queryable.Provider; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartData Visualization
        {
            get { return _visualization; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum XAxisMode
    {
        Category,
        Time
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ChartSeriesType
    {
        Point = 10,
        Line = 20,
        Area = 30,
        Bar = 40,
        StackedBar = 50
    }
}
