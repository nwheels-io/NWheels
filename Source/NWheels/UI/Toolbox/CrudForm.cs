using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "CrudForm")]
    public class CrudForm<TEntity, TData, TState> : WidgetBase<CrudForm<TEntity, TData, TState>, TData, TState>
        where TEntity : class
        where TData : class
        where TState : class
    {
        private readonly List<string> _visibleFields;
        private readonly List<string> _hiddenFields;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm(string idName, ControlledUidlNode parent)
            : this(idName, parent, isNested: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm(string idName, ControlledUidlNode parent, bool isNested)
            : base(idName, parent)
        {
            this.WidgetType = "CrudForm";
            this.TemplateName = "CrudForm";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");
            this.Fields = new List<CrudFormField>();
            this.IsInline = isNested;

            _visibleFields = new List<string>();
            _hiddenFields = new List<string>();

            this.SearchResultsReceived = new UidlNotification<string>("SearchResultsReceived", this);
            base.Notifications.Add(this.SearchResultsReceived);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> Field(
            Expression<Func<TEntity, object>> propertySelector,
            CrudFieldType type = CrudFieldType.Default,
            CrudFieldModifiers modifiers = CrudFieldModifiers.None)
        {
            var field = new CrudFormField(propertySelector.GetPropertyInfo().Name) {
                FieldType = type,
                Modifiers = modifiers
            };

            Fields.Add(field);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> ShowFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _visibleFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> HideFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _hiddenFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> Lookup<TLookupEntity>(
            Expression<Func<TEntity, object>> fieldSelector,
            Expression<Func<TLookupEntity, object>> lookupValueProperty,
            Expression<Func<TLookupEntity, object>> lookupDisplayProperty,
            Expression<Func<ViewModel<TData, TState, Empty.Input>, bool>> filterExpression = null,
            bool applyDistinctToResults = true)
        {
            var field = FindOrAddField(fieldSelector);
            var lookupMetaType = MetadataCache.GetTypeMetadata(typeof(TLookupEntity));
            
            field.LookupEntityName = lookupMetaType.Name;
            field.LookupEntityContract = typeof(TLookupEntity);
            field.LookupValueProperty = lookupValueProperty.GetPropertyInfo().Name;
            field.LookupDisplayProperty = lookupDisplayProperty.GetPropertyInfo().Name;
            field.ApplyDistinctToLookup = applyDistinctToResults;

            field.FieldType = CrudFieldType.Lookup;
            field.Modifiers = CrudFieldModifiers.DropDown;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> SetMode(CrudFormMode value)
        {
            this.Mode = value;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Fields.Select(f => f.PropertyName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public bool IsInline { get; set; }
        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string EntityMetaType { get; set; }
        [DataMember]
        public object EntityId { get; set; }
        [DataMember]
        public CrudFormMode Mode { get; set; }
        [DataMember]
        public List<CrudFormField> Fields { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<string> SearchResultsReceived { get; set; }
        public UidlNotification<TEntity> Saving { get; set; }
        public UidlNotification<TEntity> Rejecting { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudForm<TEntity, TData, TState>, TData, TState> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            BuildFields(builder);
            EntityMetaType = builder.RegisterMetaType(typeof(TEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return this.Fields.SelectMany(f => f.GetNestedWidgets());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildFields(UidlBuilder builder)
        {
            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));
            var fieldsToAdd = new List<string>();

            if ( _visibleFields.Count > 0 )
            {
                fieldsToAdd.AddRange(_visibleFields.Where(f => !fieldsToAdd.Contains(f)));
            }
            else
            {
                fieldsToAdd.AddRange(metaType.Properties.Where(ShouldAutoIncludeField).Select(p => p.Name).Where(f => !fieldsToAdd.Contains(f)));
            }

            fieldsToAdd = fieldsToAdd.Where(f => !_hiddenFields.Contains(f)).ToList();
            fieldsToAdd = fieldsToAdd.Where(f => !Fields.Select(p => p.PropertyName).Contains(f)).ToList();

            Fields.AddRange(fieldsToAdd.Select(f => new CrudFormField(f)));

            foreach ( var field in Fields )
            {
                field.Build(builder, this, metaType);
            }

            Fields.Sort((x, y) => y.OrderIndex.CompareTo(x.OrderIndex));
            builder.BuildNodes(this.Fields.SelectMany(f => f.GetNestedWidgets()).Cast<AbstractUidlNode>().ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldAutoIncludeField(IPropertyMetadata property)
        {
            //if ( property.Kind == PropertyKind.Relation && property.Relation.Kind.IsIn(RelationKind.CompositionParent, RelationKind.AggregationParent) )
            //{
            //    return false;
            //}

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CrudFormField FindOrAddField(Expression<Func<TEntity, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetPropertyInfo().Name;
            var field = Fields.FirstOrDefault(f => f.PropertyName == propertyName);

            if ( field == null )
            {
                field = new CrudFormField(propertyName);
                Fields.Add(field);
            }

            return field;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class CrudFormField
    {
        public CrudFormField(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string PropertyName { get; set; }
        [DataMember]
        public CrudFieldType FieldType { get; set; }
        [DataMember]
        public CrudFieldModifiers Modifiers { get; set; }
        [DataMember]
        public string LookupEntityName { get; set; }
        [DataMember]
        public string LookupEntityMetaType { get; set; }
        [DataMember]
        public string LookupFilterExpression { get; set; }
        [DataMember]
        public string LookupValueProperty { get; set; }
        [DataMember]
        public string LookupDisplayProperty { get; set; }
        [DataMember]
        public bool ApplyDistinctToLookup { get; set; }
        [DataMember]
        public List<string> StandardValues { get; set; }
        [DataMember]
        public bool StandardValuesExclusive { get; set; }
        [DataMember]
        public bool StandardValuesMultiple { get; set; }
        [DataMember, ManuallyAssigned]
        public WidgetUidlNode NestedWidget { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IPropertyMetadata MetaProperty { get; private set; }
        internal int OrderIndex { get; private set; }
        internal Type LookupEntityContract { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Build(UidlBuilder builder, ControlledUidlNode parent, ITypeMetadata metaType)
        {
            this.MetaProperty = metaType.GetPropertyByName(this.PropertyName);

            if ( this.FieldType == CrudFieldType.Default )
            {
                this.FieldType = GetDefaultFieldType();
                this.Modifiers = GetDefaultModifiers(this.FieldType);
            }

            if ( MetaProperty.Relation != null && MetaProperty.Relation.RelatedPartyType != null )
            {
                this.LookupEntityName = MetaProperty.Relation.RelatedPartyType.Name;
                this.LookupEntityContract = MetaProperty.Relation.RelatedPartyType.ContractType;
                this.LookupEntityMetaType = MetaProperty.Relation.RelatedPartyType.ContractType.AssemblyQualifiedNameNonVersioned();
                this.NestedWidget = CreateNestedWidget(parent, MetaProperty.Relation.RelatedPartyType);
            }

            if ( this.LookupEntityContract != null )
            {
                this.LookupEntityMetaType = builder.RegisterMetaType(this.LookupEntityContract);
            }

            if ( this.MetaProperty.ClrType.IsEnum )
            {
                builder.RegisterMetaType(this.MetaProperty.ClrType);
                this.StandardValues = Enum.GetNames(this.MetaProperty.ClrType).ToList();
                this.StandardValuesExclusive = true;
            }

            this.OrderIndex = GetOrderIndex();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            if ( this.NestedWidget != null )
            {
                return new WidgetUidlNode[] { this.NestedWidget };
            }
            else
            {
                return new WidgetUidlNode[0];
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<CrudFormFieldNestedWidget> CreateNestedWidgets(ControlledUidlNode parent)
        {
            var relatedMetaType = this.MetaProperty.Relation.RelatedPartyType;
            var allConcreteTypes = new List<ITypeMetadata>();

            if ( !relatedMetaType.IsAbstract )
            {
                allConcreteTypes.Add(relatedMetaType);
            }

            allConcreteTypes.AddRange(relatedMetaType.DerivedTypes.Where(t => !t.IsAbstract));

            var results = allConcreteTypes.Select(
                concreteType => new CrudFormFieldNestedWidget() {
                    MetaType = concreteType.ContractType.AssemblyQualifiedNameNonVersioned(),
                    Widget = CreateNestedWidget(parent, concreteType)
                }).ToList();

            return results;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private WidgetUidlNode CreateNestedWidget(ControlledUidlNode parent, ITypeMetadata nestedMetaType)
        {
            WidgetUidlNode widgetInstance = null;
            Type widgetClosedType;

            switch ( this.FieldType )
            {
                case CrudFieldType.LookupMany:
                    widgetClosedType = typeof(Crud<>).MakeGenericType(nestedMetaType.ContractType);
                    widgetInstance = (WidgetUidlNode)Activator.CreateInstance(widgetClosedType, "Nested" + this.PropertyName, parent, CrudGridMode.LookupMany);
                    break;
                case CrudFieldType.InlineGrid:
                    widgetClosedType = typeof(Crud<>).MakeGenericType(nestedMetaType.ContractType);
                    widgetInstance = (WidgetUidlNode)Activator.CreateInstance(widgetClosedType, "Nested" + this.PropertyName, parent, CrudGridMode.Inline);
                    break;
                case CrudFieldType.InlineForm:
                    widgetInstance = UidlUtility.CreateFormOrTypeSelector(nestedMetaType, "Form", parent, isInline: true);
                    break;
            }

            return widgetInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CrudFieldType GetDefaultFieldType()
        {
            switch ( this.MetaProperty.Kind )
            {
                case PropertyKind.Scalar:
                    if ( MetaProperty.Role.IsIn(PropertyRole.Key, PropertyRole.Version) || MetaProperty.IsCalculated )
                    {
                        return CrudFieldType.Label;
                    }
                    else
                    {
                        return (MetaProperty.ClrType.IsEnum ? CrudFieldType.Lookup : CrudFieldType.Edit);
                    }
                case PropertyKind.Part:
                    return (MetaProperty.IsCollection ? CrudFieldType.InlineGrid : CrudFieldType.InlineForm);
                case PropertyKind.Relation:
                    if ( MetaProperty.Relation.Kind == RelationKind.Composition || MetaProperty.Relation.RelatedPartyType.IsEntityPart )
                    {
                        return (MetaProperty.IsCollection ? CrudFieldType.InlineGrid : CrudFieldType.InlineForm);
                    }
                    else
                    {
                        return (MetaProperty.IsCollection ? CrudFieldType.LookupMany : CrudFieldType.Lookup);
                    }
                default:
                    return CrudFieldType.Default;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CrudFieldModifiers GetDefaultModifiers(CrudFieldType type)
        {
            switch ( type )
            {
                case CrudFieldType.Label:
                    return CrudFieldModifiers.ReadOnly | (MetaProperty.IsCalculated ? CrudFieldModifiers.None : CrudFieldModifiers.System);
                case CrudFieldType.Edit:
                    return CrudFieldModifiers.None;
                case CrudFieldType.Lookup:
                    return CrudFieldModifiers.DropDown;
                case CrudFieldType.LookupMany:
                    return CrudFieldModifiers.Tab;
                case CrudFieldType.InlineForm:
                    return CrudFieldModifiers.Tab;
                case CrudFieldType.InlineGrid:
                    return CrudFieldModifiers.Tab;
                default:
                    return CrudFieldModifiers.None;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int GetOrderIndex()
        {
            int value = 0;

            if ( Modifiers.HasFlag(CrudFieldModifiers.System) )
            {
                value |= 0x1000;
            }

            if ( MetaProperty.DeclaringContract.DefaultDisplayProperties.Contains(MetaProperty) )
            {
                value |= 0x800;
            }

            if ( Modifiers.HasFlag(CrudFieldModifiers.ReadOnly) )
            {
                value |= 0x400;
            }

            switch ( FieldType )
            {
                case CrudFieldType.Lookup:
                    value |= 0x200;
                    break;
                case CrudFieldType.Edit:
                    value |= 0x100;
                    break;
                case CrudFieldType.InlineForm:
                    value |= 0x80;
                    break;
                case CrudFieldType.InlineGrid:
                    value |= 0x40;
                    break;
                case CrudFieldType.LookupMany:
                    value |= 0x20;
                    break;
            }

            return value;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class CrudFormFieldNestedWidget
    {
        [DataMember]
        public string MetaType { get; set; }
        [DataMember]
        public WidgetUidlNode Widget { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CrudFormMode
    {
        CrudWidget,
        StandaloneView,
        StandaloneCreate,
        StandaloneUpdate
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CrudFieldType
    {
        Default,
        Label,
        Edit,
        Lookup,
        LookupMany,
        InlineGrid,
        InlineForm,
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [Flags]
    public enum CrudFieldModifiers
    {
        None = 0x00,
        ReadOnly = 0x01,
        DropDown = 0x02,
        TypeAhead = 0x04,
        Ellipsis = 0x08,
        Section = 0x10,
        Tab = 0x20,
        System = 0x200
    }
}
