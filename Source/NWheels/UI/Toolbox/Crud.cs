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
    public class Crud : WidgetBase<Gauge, Empty.Data, Empty.State>
    {
        public Crud(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Gauge, Empty.Data, Empty.State> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string EntityName { get; set; }
        [DataMember]
        public string EntityMetaType { get; set; }
        [DataMember]
        public List<string> DisplayColumns { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(DisplayColumns ?? new List<string>());
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
