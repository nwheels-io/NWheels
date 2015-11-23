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
        private UidlNotification _updateSource;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Gauge(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables()
                .ConcatIf(BadgeText)
                .ConcatIf(Label)
                .ConcatIf(ChangeLabel);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string BadgeText { get; set; }
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public GaugeChangeType ChangeType { get; set; }
        [DataMember]
        public string ChangeLabel { get; set; }
        [DataMember]
        public string ValueDataProperty { get; set; }
        [DataMember]
        public string OldValueDataProperty { get; set; }
        [DataMember]
        public string UpdateSourceQualifiedName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ManuallyAssigned]
        public UidlNotification UpdateSource {
            get
            {
                return _updateSource;
            }
            set
            {
                _updateSource = value;
                UpdateSourceQualifiedName = (value != null ? value.QualifiedName : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.BuildManuallyInstantiatedNodes(UpdateSource);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, IGaugeState> presenter)
        {
            presenter.On(UpdateSource).AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.Data));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IGaugeState
        {
            Empty.Payload Data { get; set; }
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

        public static Gauge Appearance(this Gauge gauge, string label, GaugeChangeType changeType = GaugeChangeType.None, string changeLabel = null)
        {
            gauge.Label = label;
            gauge.ChangeType = changeType;
            gauge.ChangeLabel = changeLabel;

            return gauge;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Gauge DataSource<TPayload>(
            this Gauge gauge, 
            UidlNotification<TPayload> updateSource, 
            Expression<Func<TPayload, int>> valueProperty, 
            Expression<Func<TPayload, int>> oldValueProperty = null)
        {
            gauge.UpdateSource = updateSource;
            gauge.ValueDataProperty = valueProperty.ToNormalizedNavigationString(false, "input").TrimLead("input.");
            gauge.OldValueDataProperty = (oldValueProperty != null ? oldValueProperty.ToNormalizedNavigationString(false, "input").TrimLead("input.") : null);
            
            return gauge;
        }
    }
}
