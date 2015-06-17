using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.UI.Toolbox
{
    public class EntityFormScreenPart<TEntity> : ScreenPartComponent<
        EntityFormScreenPart<TEntity>, 
        IEntityId<TEntity>,
        EntityFormScreenPart<TEntity>.IData,
        EntityFormScreenPart<TEntity>.IState>
        where TEntity : class
    {
        public override void DescribePresenter(IScreenPartPresenter<EntityFormScreenPart<TEntity>, IEntityId<TEntity>, IData, IState> presenter)
        {
            presenter.On(NavigatedHere)
                .AlterModel().SetState(s => s.EntityId, (data, state, input) => input)
                .Then(b => b.CallApi<IEntityApi<TEntity>>().RequestReply<TEntity>((api, data, state, input) => api.RetrieveById(input)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Form TheForm { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IData
        {
            TEntity Entity { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IState
        {
            IEntityId<TEntity> EntityId { get; set; }
        }
    }
}
