namespace NWheels.Authorization.Core
{
    public interface IAccessControlList
    {
        IEntityAccessControl<TEntity> GetEntityAccessControl<TEntity>();
    }
}
