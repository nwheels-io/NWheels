using NWheels.Authorization;

namespace NWheels.UI.OldToolbox
{
    public class EntityCrud<TEntity> : ScreenPartComponent<EntityCrud<TEntity>, Empty.Data, Empty.State>
        where TEntity : class
    {
        public override void DescribePresenter(IScreenPartPresenter<EntityCrud<TEntity>, Empty.Input, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = EntityTable;
            //presenter.OnChange(view => view.EntityTable.SelectedItem).CallApi<ISecurityDomainApi>().RequestReply<bool>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Table<TEntity> EntityTable { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AuthorizationContract.RequirePerResourceType(typeof(EntityCrud<>), EntityPermission.Create)]
        public ICommand Add { get; set; }

        public ICommand Open { get; set; }

        [AuthorizationContract.RequirePerResourceType(typeof(EntityCrud<>), EntityPermission.Update)]
        public ICommand Update { get; set; }

        [AuthorizationContract.RequirePerResourceType(typeof(EntityCrud<>), EntityPermission.Delete)]
        public ICommand Delete { get; set; }
    }
}
