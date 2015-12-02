using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI;
using NWheels.UI.Core;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class TypeSelector : WidgetBase<TypeSelector, Empty.Data, Empty.State>
    {
        public TypeSelector(string idName, ControlledUidlNode parent, ITypeMetadata baseMetaType, IEnumerable<ITypeMetadata> availableConcreteTypes)
            : base(idName, parent)
        {
            this.BaseMetaType = baseMetaType;
            this.BaseTypeName = baseMetaType.Name;
            this.Selections = availableConcreteTypes.Select(t => new Selection(t)).ToList();
            this.ParentModelProperty = "Entity";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Selections.Select(s => s.Text));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WidgetUidlNode GetWidget(Type contract)
        {
            return this.Selections.Where(s => s.MetaType.ContractType == contract).Select(s => s.Widget).First();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TWidget GetWidget<TContract, TWidget>()
            where TContract : class
            where TWidget : WidgetUidlNode
        {
            return this.Selections.Where(s => s.MetaType.ContractType == typeof(TContract)).Select(s => s.Widget).Cast<TWidget>().First();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SetWidget(Type contract, WidgetUidlNode widget)
        {
            var selection = this.Selections.First(s => s.MetaType.ContractType == contract);
            selection.Widget = widget;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TWidget> WidgetsOfType<TWidget>()
        {
            return Selections.Select(s => s.Widget).OfType<TWidget>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ForEachWidget(Action<WidgetUidlNode> action)
        {
            foreach ( var widget in Selections.Select(s => s.Widget) )
            {
                action(widget);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ForEachWidgetOfType<TWidget>(Action<TWidget> action)
        {
            foreach ( var widget in Selections.Select(s => s.Widget).OfType<TWidget>() )
            {
                action(widget);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FilterByType(Type ancestor)
        {
            for ( var i = Selections.Count - 1 ; i >= 0 ; i-- )
            {
                if ( !ancestor.IsAssignableFrom(Selections[i].MetaType.ContractType) )
                {
                    Selections.RemoveAt(i);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UseTabStyle()
        {
            this.TemplateName = "TypeSelectorWithTabs";

            if ( Selections.Count > 0 )
            {
                this.DefaultTypeName = Selections.First().TypeName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UseAccordionStyle()
        {
            this.TemplateName = "TypeSelectorWithAccordion";

            if ( Selections.Count > 0 )
            {
                this.DefaultTypeName = Selections.First().TypeName;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string BaseTypeName { get; set; }
        [DataMember, ManuallyAssigned]
        public List<Selection> Selections { get; set; }
        [DataMember]
        public string DefaultTypeName { get; set; }
        [DataMember]
        public string ParentModelProperty { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification ModelSetter { get; set; }
        public UidlNotification StateResetter { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TypeSelector, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.BuildManuallyInstantiatedNodes(Selections.Select(s => s.Widget).Cast<AbstractUidlNode>().ToArray());

            foreach ( var concreteType in Selections.Select(s => s.MetaType) )
            {
                builder.RegisterMetaType(concreteType.ContractType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return Selections.Select(s => s.Widget);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ITypeMetadata BaseMetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static TypeSelector Create(
            string idName, 
            ControlledUidlNode parent, 
            ITypeMetadata baseType, 
            IEnumerable<ITypeMetadata> availableConcreteTypes,
            Func<ITypeMetadata, WidgetUidlNode> concreteWidgetFactory)
        {
            var concreteTypesArray = availableConcreteTypes.ToArray();
            var selector = new TypeSelector(idName, parent, baseType, concreteTypesArray);

            foreach ( var concreteType in concreteTypesArray )
            {
                selector.SetWidget(concreteType.ContractType, concreteWidgetFactory(concreteType));
            }

            return selector;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = UidlDocument.DataContractNamespace)]
        public class Selection
        {
            public Selection(ITypeMetadata metaType)
            {
                this.Text = (metaType.BaseType != null ? metaType.Name.TrimTail(metaType.BaseType.Name.TrimLead("Abstract")) : metaType.Name);
                this.MetaType = metaType;
                this.TypeName = metaType.QualifiedName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public string TypeName { get; set; }
            [DataMember]
            public WidgetUidlNode Widget { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal ITypeMetadata MetaType { get; private set; }
        }
    }
}
