using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Gauge : WidgetBase<Gauge, Empty.Data, Gauge.IGaugeState>
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
                .ConcatIf(AlertText)
                .Concat(Values.SelectMany(v => new[] { v.Title, v.AlertText }).Where(s => s != null));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Builder<TModel> BindToModelSetter<TModel>(UidlNotification<TModel> modelSetter)
        {
            ModelSetterQualifiedName = modelSetter.QualifiedName;
            return new Builder<TModel>(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public UserAlertType? AlertType { get; set; }
        [DataMember]
        public string AlertText { get; set; }
        [DataMember]
        public string AlertTextProperty { get; set; }
        [DataMember]
        public string AlertIcon { get; set; }
        [DataMember]
        public string AlertIconProperty { get; set; }
        [DataMember]
        public List<GaugeValue> Values { get; set; }
        [DataMember]
        public string ModelSetterQualifiedName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, IGaugeState> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static string GetExpressionString<TModel>(Expression<Func<TModel, object>> expression)
        {
            if ( expression != null )
            {
                return expression.ToNormalizedNavigationString("input");
            }
            else
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IGaugeState
        {
            Empty.Payload Data { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Builder<TModel>
        {
            private readonly Gauge _gauge;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder(Gauge gauge)
            {
                _gauge = gauge;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Title(string title)
            {
                _gauge.Text = title;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Alert(
                UserAlertType alertType, 
                string text = null, 
                string icon = null, 
                Expression<Func<TModel, object>> textProperty = null,
                Expression<Func<TModel, object>> iconProperty = null)
            {
                _gauge.AlertType = alertType;
                _gauge.AlertText = text;
                _gauge.AlertIcon = icon;
                _gauge.AlertTextProperty = GetExpressionString(textProperty);
                _gauge.AlertIconProperty = GetExpressionString(iconProperty);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Value(Action<GaugeValue.Builder<TModel>> initializer)
            {
                var value = new GaugeValue();
                var valueBuilder = new GaugeValue.Builder<TModel>(value);
                initializer(valueBuilder);
                _gauge.Values.Add(value);
                return this;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class GaugeValue
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public UserAlertType TitleAlertType { get; set; }
        [DataMember]
        public object Value { get; set; }
        [DataMember]
        public string ValueProperty { get; set; }
        [DataMember]
        public UserAlertType? AlertType { get; set; }
        [DataMember]
        public object AlertValue { get; set; }
        [DataMember]
        public string AlertValueProperty { get; set; }
        [DataMember]
        public string AlertText { get; set; }
        [DataMember]
        public string AlertTextProperty { get; set; }
        [DataMember]
        public string AlertIcon { get; set; }
        [DataMember]
        public string AlertIconProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Builder<TModel>
        {
            private readonly GaugeValue _value;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder(GaugeValue value)
            {
                _value = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Title(string title)
            {
                _value.Title = title;
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Data(object value = null, Expression<Func<TModel, object>> property = null)
            {
                _value.Value = value;
                _value.ValueProperty = Gauge.GetExpressionString(property);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Builder<TModel> Alert(
                UserAlertType type, 
                object value = null, 
                string text = null, 
                string icon = null, 
                Expression<Func<TModel, object>> valueProperty = null,
                Expression<Func<TModel, object>> textProperty = null,
                Expression<Func<TModel, object>> iconProperty = null)
            {
                _value.AlertType = type;
                _value.AlertText = text;
                _value.AlertIcon = icon;
                _value.AlertValueProperty = Gauge.GetExpressionString(valueProperty);
                _value.AlertTextProperty = Gauge.GetExpressionString(textProperty);
                _value.AlertIconProperty = Gauge.GetExpressionString(iconProperty);
                return this;
            }
        }
    }
}
