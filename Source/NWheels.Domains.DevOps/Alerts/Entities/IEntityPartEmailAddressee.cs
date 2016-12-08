using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(IsAbstract = true)]
    public interface IEntityPartEmailRecipient
    {
    }

    [EntityPartContract]
    public interface IEntityPartEmailAddressRecipient : IEntityPartEmailRecipient
    {
        [PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }
    }

    [EntityPartContract]
    public interface IEntityPartUserAccountEmailRecipient : IEntityPartEmailRecipient
    {
        IUserAccountEntity User { get; set; }
    }
}
