using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class CrudForm : WidgetBase<CrudForm, Empty.Data, Empty.State>
    {
        public CrudForm(string idName, Crud parent)
            : base(idName, parent)
        {
            DisplayFields = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudForm, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string EntityMetaType { get; set; }
        [DataMember]
        public List<string> DisplayFields { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayFields);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "CrudForm")]
    public class CrudForm<TEntity> : CrudForm
        where TEntity : class
    {
        private readonly List<string> _visibleFields;
        private readonly List<string> _hiddenFields;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm(string idName, Crud<TEntity> parent)
            : base(idName, parent)
        {
            this.WidgetType = "CrudForm";
            this.TemplateName = "CrudForm";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");

            _visibleFields = new List<string>();
            _hiddenFields = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity> ShowFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _visibleFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CrudForm<TEntity> HideFields(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            _hiddenFields.AddRange(propertySelectors.Select(e => e.GetPropertyInfo().Name));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBuild(UidlBuilder builder)
        {
            BuildDisplayFields(builder);
            base.EntityMetaType = builder.RegisterMetaType(typeof(TEntity));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildDisplayFields(UidlBuilder builder)
        {
            var fieldList = new HashSet<string>();
            var metaType = builder.MetadataCache.GetTypeMetadata(typeof(TEntity));

            if ( _visibleFields.Count > 0 )
            {
                fieldList.UnionWith(_visibleFields);
            }
            else
            {
                fieldList.UnionWith(metaType.Properties.Select(p => p.Name));
            }

            fieldList.ExceptWith(_hiddenFields);

            if ( _visibleFields.Count > 0 )
            {
                base.DisplayFields.AddRange(_visibleFields.Where(f => fieldList.Contains(f)));
            }
            else
            {
                base.DisplayFields.AddRange(fieldList);
            }
        }
    }
}
