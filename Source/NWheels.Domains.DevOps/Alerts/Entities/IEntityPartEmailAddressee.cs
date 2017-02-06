using System;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------        

        [PropertyContract.Calculated]
        string SendToEmail { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------        

        [PropertyContract.Calculated]
        string UserFullName { get; }
    }

    public abstract class EntityPartEmailRecipient : IEntityPartEmailRecipient
    {
        public abstract OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------        

        public abstract string SendToEmail { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------        

        public abstract string UserFullName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartEmailAddressRecipient : IEntityPartEmailRecipient
    {
        [PropertyContract.Semantic.EmailAddress]
        string Email { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class EntityPartEmailAddressRecipient : EntityPartEmailRecipient, IEntityPartEmailAddressRecipient
    {
        public override OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient()
        {
            return new OutgoingEmailMessage.SenderRecipient(Email, Email);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string Email { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string SendToEmail
        {
            get { return Email; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string UserFullName
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

    public abstract class EntityPartUserAccountEmailRecipient : EntityPartEmailRecipient, IEntityPartUserAccountEmailRecipient
    {
        public override OutgoingEmailMessage.SenderRecipient ToOutgoingEmailMessageRecipient()
        {
            return new OutgoingEmailMessage.SenderRecipient(User.FullName ?? User.LoginName, User.EmailAddress);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IUserAccountEntity User { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string SendToEmail
        {
            get { return User == null ? null : User.EmailAddress; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string UserFullName
        {
            get { return User == null ? null : User.FullName; }
        }
    }
}
