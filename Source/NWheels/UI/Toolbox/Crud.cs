using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    [DataContract(Namespace = UidlDocument.DataContractNamespace)]
    public class Crud : WidgetBase<Crud, Empty.Data, Empty.State>
    {
        public Crud(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            DisplayColumns = new List<string>();
            Form = new CrudForm("Form", this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Crud, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string EntityMetaType { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }
        [DataMember, ManuallyAssigned]
        public CrudForm Form { get; set; }

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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [DataContract(Namespace = UidlDocument.DataContractNamespace, Name = "Crud")]
    public class Crud<TEntity> : Crud
        where TEntity : class
    {
        public Crud(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
            this.WidgetType = "Crud";
            this.TemplateName = "Crud";
            this.EntityName = typeof(TEntity).Name.TrimLead("I").TrimTail("Entity");
            this.Form = new CrudForm<TEntity>("Form", this);
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

        protected override void OnBuild(UidlBuilder builder)
        {
            base.EntityMetaType = builder.RegisterMetaType(typeof(TEntity));
        }
    }
}
