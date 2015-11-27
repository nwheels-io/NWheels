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
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Form")]
    public class Form<TEntity> : WidgetBase<Form<TEntity>, Form<TEntity>.IFormData, Empty.State>, IUidlForm
        where TEntity : class
    {
        private readonly List<string> _visibleFields;
        private readonly List<string> _hiddenFields;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form(string idName, ControlledUidlNode parent)
            : this(idName, parent, isNested: false)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form(string idName, ControlledUidlNode parent, bool isNested)
            : base(idName, parent)
        {
            this.WidgetType = "Form";
            this.TemplateName = "Form";
            this.EntityName = MetadataCache.GetTypeMetadata(typeof(TEntity)).QualifiedName;
            this.Fields = new List<FormField>();
            this.UsePascalCase = true;

            _visibleFields = new List<string>();
            _hiddenFields = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form<TEntity> Field(
            Expression<Func<TEntity, object>> propertySelector,
            FormFieldType type = FormFieldType.Default,
            FormFieldModifiers modifiers = FormFieldModifiers.None)
        {
            var field = new FormField(propertySelector.GetPropertyInfo().Name) {
                FieldType = type,
                Modifiers = modifiers
            };

            Fields.Add(field);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form<TEntity> ShowFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _visibleFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form<TEntity> HideFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _hiddenFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form<TEntity> Lookup<TLookupEntity>(
            Expression<Func<TEntity, object>> fieldSelector,
            Expression<Func<TLookupEntity, object>> lookupValueProperty,
            Expression<Func<TLookupEntity, object>> lookupDisplayProperty,
            Expression<Func<ViewModel<IFormData, Empty.State, Empty.Input>, bool>> filterExpression = null,
            bool applyDistinctToResults = true)
        {
            var field = FindOrAddField(fieldSelector);
            var lookupMetaType = MetadataCache.GetTypeMetadata(typeof(TLookupEntity));
            
            field.LookupEntityName = lookupMetaType.QualifiedName;
            field.LookupEntityContract = typeof(TLookupEntity);
            field.LookupValueProperty = lookupValueProperty.GetPropertyInfo().Name;
            field.LookupDisplayProperty = lookupDisplayProperty.GetPropertyInfo().Name;
            field.ApplyDistinctToLookup = applyDistinctToResults;

            field.FieldType = FormFieldType.Lookup;
            field.Modifiers = FormFieldModifiers.DropDown;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            var metaTypeTranslatables = new List<string>();

            for ( var metaType = MetadataCache.GetTypeMetadata(typeof(TEntity)) ; metaType != null ; metaType = metaType.BaseType )
            {
                metaTypeTranslatables.Add(metaType.QualifiedName);
            }

            return base.GetTranslatables()
                .Concat(metaTypeTranslatables)
                .Concat(Fields.Select(f => f.PropertyName))
                .Concat(Fields.Where(f => f.StandardValues != null && f.StandardValuesExclusive).SelectMany(f => f.StandardValues))
                .Concat(Commands.Select(c => c.Text)); 
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public object EntityId { get; set; }
        [DataMember]
        public List<FormField> Fields { get; set; }
        [DataMember]
        public bool UsePascalCase { get; set; }
        [DataMember]
        public bool IsModalPopup { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<TEntity> ModelSetter { get; set; }
        public UidlNotification StateResetter { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Form<TEntity>, IFormData, Empty.State> presenter)
        {
            //presenter.On(ModelSetter).AlterModel((alt => alt.Copy(vm => vm.Input).To(vm => vm.Data.Entity)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            BuildFields(builder);
            builder.RegisterMetaType(typeof(TEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return this.Fields.SelectMany(f => f.GetNestedWidgets());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IUidlForm.ShowFields(params string[] propertyNames)
        {
            _visibleFields.AddRange(propertyNames);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IUidlForm.HideFields(params string[] propertyNames)
        {
            _hiddenFields.AddRange(propertyNames);

            foreach ( var propertyName in propertyNames )
            {
                var field = this.Fields.FirstOrDefault(f => f.PropertyName == propertyName);
                
                if ( field != null )
                {
                    this.Fields.Remove(field);
                }
            }
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

            Fields.AddRange(fieldsToAdd.Select(f => new FormField(f)));

            foreach ( var field in Fields )
            {
                field.Build(builder, this, metaType);
            }

            Fields.Sort((x, y) => y.OrderIndex.CompareTo(x.OrderIndex));
            builder.BuildNodes(this.Fields.SelectMany(f => f.GetNestedWidgets()).Cast<AbstractUidlNode>().ToArray());
            builder.BuildNodes(this.Commands.Cast<AbstractUidlNode>().ToArray());
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

        private FormField FindOrAddField(Expression<Func<TEntity, object>> fieldSelector)
        {
            var propertyName = fieldSelector.GetPropertyInfo().Name;
            var field = Fields.FirstOrDefault(f => f.PropertyName == propertyName);

            if ( field == null )
            {
                field = new FormField(propertyName);
                Fields.Add(field);
            }

            return field;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IFormData
        {
            TEntity Entity { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class FormField
    {
        public FormField(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string PropertyName { get; set; }
        [DataMember]
        public FormFieldType FieldType { get; set; }
        [DataMember]
        public FormFieldModifiers Modifiers { get; set; }
        [DataMember]
        public string LookupEntityName { get; set; }
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

            if ( this.FieldType == FormFieldType.Default )
            {
                this.FieldType = GetDefaultFieldType();
                this.Modifiers = GetDefaultModifiers(this.FieldType);
            }

            if ( MetaProperty.Relation != null && MetaProperty.Relation.RelatedPartyType != null )
            {
                this.LookupEntityName = MetaProperty.Relation.RelatedPartyType.QualifiedName;
                this.LookupEntityContract = MetaProperty.Relation.RelatedPartyType.ContractType;
                this.NestedWidget = CreateNestedWidget(parent, MetaProperty.Relation.RelatedPartyType);
            }

            if ( this.LookupEntityContract != null )
            {
                builder.RegisterMetaType(this.LookupEntityContract);
            }

            if ( this.MetaProperty.ClrType.IsEnum )
            {
                builder.RegisterMetaType(this.MetaProperty.ClrType);
                this.StandardValues = Enum.GetNames(this.MetaProperty.ClrType).ToList();
                this.StandardValuesExclusive = true;
            }

            this.OrderIndex = GetOrderIndex();

            builder.BuildManuallyInstantiatedNodes(NestedWidget);
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

        private List<FormFieldNestedWidget> CreateNestedWidgets(ControlledUidlNode parent)
        {
            var relatedMetaType = this.MetaProperty.Relation.RelatedPartyType;
            var allConcreteTypes = new List<ITypeMetadata>();

            if ( !relatedMetaType.IsAbstract )
            {
                allConcreteTypes.Add(relatedMetaType);
            }

            allConcreteTypes.AddRange(relatedMetaType.DerivedTypes.Where(t => !t.IsAbstract));

            var results = allConcreteTypes.Select(
                concreteType => new FormFieldNestedWidget() {
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
                case FormFieldType.LookupMany:
                    widgetClosedType = typeof(LookupGrid<,>).MakeGenericType(nestedMetaType.EntityIdProperty.ClrType, nestedMetaType.ContractType);
                    widgetInstance = (WidgetUidlNode)Activator.CreateInstance(widgetClosedType, "Nested" + this.PropertyName + "Grid", parent);
                    break;
                case FormFieldType.InlineGrid:
                    widgetClosedType = typeof(Crud<>).MakeGenericType(nestedMetaType.ContractType);
                    widgetInstance = (WidgetUidlNode)Activator.CreateInstance(widgetClosedType, "Nested" + this.PropertyName + "Crud", parent, DataGridMode.Inline);
                    break;
                case FormFieldType.InlineForm:
                    widgetInstance = UidlUtility.CreateFormOrTypeSelector(nestedMetaType, "Nested" + this.PropertyName + "Form", parent, isInline: true);
                    break;
            }

            return widgetInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private FormFieldType GetDefaultFieldType()
        {
            switch ( this.MetaProperty.Kind )
            {
                case PropertyKind.Scalar:
                    if ( MetaProperty.Role.IsIn(PropertyRole.Key, PropertyRole.Version) || MetaProperty.IsCalculated )
                    {
                        return (MetaProperty.ContractPropertyInfo.CanWrite ? FormFieldType.Edit : FormFieldType.Label);
                    }
                    else
                    {
                        return (MetaProperty.ClrType.IsEnum ? FormFieldType.Lookup : FormFieldType.Edit);
                    }
                case PropertyKind.Part:
                    return (MetaProperty.IsCollection ? FormFieldType.InlineGrid : FormFieldType.InlineForm);
                case PropertyKind.Relation:
                    if ( MetaProperty.Relation.Kind == RelationKind.Composition || MetaProperty.Relation.RelatedPartyType.IsEntityPart )
                    {
                        return (MetaProperty.IsCollection ? FormFieldType.InlineGrid : FormFieldType.InlineForm);
                    }
                    else
                    {
                        return (MetaProperty.IsCollection ? FormFieldType.LookupMany : FormFieldType.Lookup);
                    }
                default:
                    return FormFieldType.Default;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private FormFieldModifiers GetDefaultModifiers(FormFieldType type)
        {
            switch ( type )
            {
                case FormFieldType.Label:
                    return FormFieldModifiers.ReadOnly | (MetaProperty.IsCalculated ? FormFieldModifiers.None : FormFieldModifiers.System);
                case FormFieldType.Edit:
                    if ( MetaProperty.ClrType == typeof(Boolean) )
                    {
                        return FormFieldModifiers.Checkbox;
                    }
                    else if ( MetaProperty.ClrType == typeof(DateTime) )
                    {
                        return FormFieldModifiers.DateTimePicker;
                    }
                    else if ( MetaProperty.IsSensitive )
                    {
                        return FormFieldModifiers.Password;
                    }
                    return FormFieldModifiers.None;
                case FormFieldType.Lookup:
                    return FormFieldModifiers.DropDown;
                case FormFieldType.LookupMany:
                    return FormFieldModifiers.Tab;
                case FormFieldType.InlineForm:
                    return FormFieldModifiers.Tab;
                case FormFieldType.InlineGrid:
                    return FormFieldModifiers.Tab;
                default:
                    return FormFieldModifiers.None;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private int GetOrderIndex()
        {
            int value = 0;

            if ( Modifiers.HasFlag(FormFieldModifiers.System) )
            {
                value |= 0x1000;
            }

            if ( MetaProperty.DeclaringContract.DefaultDisplayProperties.Contains(MetaProperty) )
            {
                value |= 0x800;
            }

            if ( Modifiers.HasFlag(FormFieldModifiers.ReadOnly) )
            {
                value |= 0x400;
            }

            switch ( FieldType )
            {
                case FormFieldType.Lookup:
                    value |= 0x200;
                    break;
                case FormFieldType.Edit:
                    value |= 0x100;
                    break;
                case FormFieldType.InlineForm:
                    value |= 0x80;
                    break;
                case FormFieldType.InlineGrid:
                    value |= 0x40;
                    break;
                case FormFieldType.LookupMany:
                    value |= 0x20;
                    break;
            }

            return value;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public interface IUidlForm
    {
        void ShowFields(params string[] propertyNames);
        void HideFields(params string[] propertyNames);
        bool UsePascalCase { get; set; }
        List<FormField> Fields { get; }
        List<UidlCommand> Commands { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class FormFieldNestedWidget
    {
        [DataMember]
        public string MetaType { get; set; }
        [DataMember]
        public WidgetUidlNode Widget { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum FormFieldType
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
    public enum FormFieldModifiers
    {
        None = 0x00,
        ReadOnly = 0x01,
        DropDown = 0x02,
        TypeAhead = 0x04,
        Ellipsis = 0x08,
        Section = 0x10,
        Tab = 0x20,
        Checkbox = 0x40,
        DateTimePicker = 0x80,
        Password = 0x100,
        System = 0x2000
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum FieldSize
    {
        Small,
        Medium,
        Large
    }
}
