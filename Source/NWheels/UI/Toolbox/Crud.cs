using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class Crud<TEntity> : WidgetBase<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>>
        where TEntity : class
    {
        public Crud(string idName, ControlledUidlNode parent)
            : this(idName, parent, CrudGridMode.Standalone)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud(string idName, ControlledUidlNode parent, CrudGridMode mode)
            : base(idName, parent)
        {
            this.WidgetType = "Crud";
            this.TemplateName = "Crud";
            this.MetaType = this.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.EntityName = MetaType.QualifiedName;
            this.Mode = mode;
            this.DisplayColumns = new List<string>();

            CreateForm();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns ?? new List<string>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return new WidgetUidlNode[] { Form, FormTypeSelector }.Where(w => w != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud<TEntity> Column<T>(Expression<Func<TEntity, T>> propertySelector)
        {
            var property = propertySelector.GetPropertyInfo();

            if ( this.DisplayColumns == null )
            {
                this.DisplayColumns = new List<string>();
            }

            this.DisplayColumns.Add(property.Name);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public CrudGridMode Mode { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }
        [DataMember]
        public List<string> DefaultDisplayColumns { get; set; }
        [DataMember, ManuallyAssigned]
        public CrudForm<TEntity, Empty.Data, ICrudFormState<TEntity>> Form { get; set; }
        [DataMember, ManuallyAssigned]
        public TypeSelector FormTypeSelector { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            builder.RegisterMetaType(typeof(TEntity));

            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.DefaultDisplayColumns = metaType.Properties
                .Where(p => p.Kind == PropertyKind.Scalar && p.Role == PropertyRole.None)
                .Select(p => p.Name)
                .ToList();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ITypeMetadata MetaType { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void CreateForm()
        {
            var formOrTypeSelector = UidlUtility.CreateFormOrTypeSelector(MetaType, "Form", parent: this, isInline: false);

            this.Form = formOrTypeSelector as CrudForm<TEntity, Empty.Data, ICrudFormState<TEntity>>;
            this.FormTypeSelector = formOrTypeSelector as TypeSelector;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public enum CrudGridMode
    {
        Standalone,
        LookupMany,
        Inline
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudViewState<TEntity>
    {
        TEntity CurrentEntity { get; set; }
        ICollection<ICrudNavigatedEntity> NavigatedEntities { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudNavigatedEntity
    {
        Type Contract { get; set; }
        string Name { get; set; }
        object Id { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    [ViewModelContract]
    public interface ICrudFormState<TEntity>
    {
        TEntity CurrentEntity { get; set; }
        ICollection<ICrudNavigatedEntity> NavigatedEntities { get; set; }
    }
}
