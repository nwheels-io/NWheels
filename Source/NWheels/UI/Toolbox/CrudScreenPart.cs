using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class CrudScreenPart<TEntity> : ScreenPartBase<CrudScreenPart<TEntity>, Empty.Input, Empty.Data, Empty.State>
        where TEntity : class
    {
        public CrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GridColumns(params Expression<Func<TEntity, object>>[] propertySelectors)
        {
            foreach ( var property in propertySelectors )
            {
                Crud.Column(property);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<TEntity>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Crud;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud<TEntity> Crud { get; set; }
    }

}
