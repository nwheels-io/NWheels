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
            : base(idName, parent)
        {
            this.WidgetType = "Crud";
            this.TemplateName = "Crud";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");
            this.Form = new CrudForm<TEntity, Empty.Data, ICrudFormState<TEntity>>("Form", this);
            this.DisplayColumns = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns ?? new List<string>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<WidgetUidlNode> GetNestedWidgets()
        {
            return new WidgetUidlNode[] { Form };
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
        public string EntityMetaType { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }
        [DataMember]
        public List<string> DefaultDisplayColumns { get; set; }
        [DataMember, ManuallyAssigned]
        public CrudForm<TEntity, Empty.Data, ICrudFormState<TEntity>> Form { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Crud<TEntity>, Empty.Data, ICrudViewState<TEntity>> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            this.EntityMetaType = builder.RegisterMetaType(typeof(TEntity));

            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.DefaultDisplayColumns = metaType.Properties
                .Where(p => p.Kind == PropertyKind.Scalar && p.Role == PropertyRole.None)
                .Select(p => p.Name)
                .ToList();
        }
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
        string MetaType { get; set; }
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
