using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Authorization.Impl
{
    public interface IUserStorageContext : IApplicationDataRepository
    {
        IEntityRepository<IUserStorageItemEntity> UserStorageItems { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IUserStorageItemEntity
    {
        [PropertyContract.Required]
        [PropertyContract.UniqueKey(Name = "UserItemKey")]
        string UserId { get; set; }

        [PropertyContract.Required]
        [PropertyContract.UniqueKey(Name = "UserItemKey")]
        string Key { get; set; }

        string Value { get; set; }
    }
}
