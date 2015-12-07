using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class CrudScreenPart<TEntity> : ScreenPartBase<CrudScreenPart<TEntity>, Empty.Input, Empty.Data, CrudScreenPart<TEntity>.IState>
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
                Crud.Grid.Column<object>(property);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<TEntity>, Empty.Data, IState> presenter)
        {
            ContentRoot = Crud;

            var metaType = base.MetadataCache.GetTypeMetadata(typeof(TEntity));
            this.Text = metaType.Name + "Management";

            presenter.On(Crud.SelectedEntityChanged).AlterModel(alt => alt.Copy(vm => vm.Input).To(vm => vm.State.Entity));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Crud<TEntity> Crud { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            TEntity Entity { get; set; }
        }
    }
}
