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
            : base(idName, parent)
        {
            this.WidgetType = "CrudForm";
            this.TemplateName = "CrudForm";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");
            this.Fields = new List<CrudFormField>();

            _visibleFields = new List<string>();
            _hiddenFields = new List<string>();
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

        public CrudForm<TEntity, TData, TState> FilterLookup<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity>> propertySelector,
            Expression<Func<ViewModel<TData, TState, Empty.Input>, bool>> filterExpression)
        {
            SetLookupEntityContract(propertySelector, typeof(TRelatedEntity));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity, TData, TState> FilterLookup<TRelatedEntity>(
            Expression<Func<TEntity, ICollection<TRelatedEntity>>> propertySelector,
            Expression<Func<ViewModel<TData, TState, Empty.Input>, bool>> filterExpression)
        {
            SetLookupEntityContract(propertySelector, typeof(TRelatedEntity));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(Fields.Select(f => f.PropertyName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        private void BuildFields(UidlBuilder builder)
        {
            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));
            var fieldsToAdd = new HashSet<string>();

            if (_visibleFields.Count > 0)
            {
                fieldsToAdd.UnionWith(_visibleFields);
            }
            else
            {
                fieldsToAdd.UnionWith(metaType.Properties.Select(p => p.Name));
            }

            fieldsToAdd.ExceptWith(_hiddenFields);
            fieldsToAdd.ExceptWith(Fields.Select(f => f.PropertyName));

            Fields.AddRange(fieldsToAdd.Select(f => new CrudFormField(f)));

            foreach (var field in Fields)
            {
                field.Build(metaType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetLookupEntityContract<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> propertySelector, Type lookupEntityContract)
        {
            var propertyName = propertySelector.GetPropertyInfo().Name;
            var field = Fields.FirstOrDefault(f => f.PropertyName == propertyName);

            if ( field == null )
            {
                field = new CrudFormField(propertyName);
                Fields.Add(field);
            }

            field.LookupEntityContract = lookupEntityContract;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IPropertyMetadata MetaProperty { get; private set; }
        internal Type LookupEntityContract { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Build(ITypeMetadata metaType)
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
                this.LookupEntityMetaType = MetaProperty.Relation.RelatedPartyType.ContractType.AssemblyQualifiedNameNonVersioned();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private CrudFieldType GetDefaultFieldType()
        {
            switch ( this.MetaProperty.Kind )
            {
                case PropertyKind.Scalar:
                    return CrudFieldType.Edit;
                case PropertyKind.Part:
                    return CrudFieldType.NestedForm;
                case PropertyKind.Relation:
                    return (MetaProperty.IsCollection ? CrudFieldType.LookupMany : CrudFieldType.Lookup);
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
                    return CrudFieldModifiers.ReadOnly;
                case CrudFieldType.Edit:
                    return CrudFieldModifiers.None;
                case CrudFieldType.Lookup:
                    return CrudFieldModifiers.DropDown;
                case CrudFieldType.LookupMany:
                    return CrudFieldModifiers.Tab;
                case CrudFieldType.NestedForm:
                    return CrudFieldModifiers.Tab;
                default:
                    return CrudFieldModifiers.None;
            }
        }
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
        NestedForm
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
        Tab = 0x20
    }
}
