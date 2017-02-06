using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Processing.Messages;

namespace NWheels.Domains.DevOps.Alerts.Entities
{
    [EntityPartContract(IsAbstract = true)]
    public interface IEntityPartEmailRecipient
    {
        OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient();

        [PropertyContract.Calculated]
        string SendToEmail { get; }

        [PropertyContract.Calculated]
        string UserFullName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartEmailAddressRecipient : IEntityPartEmailRecipient
    {
        [PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityPartEmailAddressRecipient : IEntityPartEmailAddressRecipient
    {
        public OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient()
        {
            return new OutgoingEmailMessage.SenderRecipient(Email, Email);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string Email { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SendToEmail
        {
            get { return Email; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserFullName
        {
            get { return Email; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartUserAccountEmailRecipient : IEntityPartEmailRecipient
    {
        IUserAccountEntity User { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityPartUserAccountEmailRecipient : IEntityPartUserAccountEmailRecipient
    {
        public OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient()
        {
            return new OutgoingEmailMessage.SenderRecipient(User.FullName ?? User.LoginName, User.EmailAddress);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IUserAccountEntity User { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SendToEmail
        {
            get { return User == null ? null : User.EmailAddress; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UserFullName
        {
            get { return User == null ? null : User.FullName; }
        }
    }
}
