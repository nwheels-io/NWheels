using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.Security
{
    public interface IUserAccountDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IUserAccountEntity> AllUsers { get; }
        IEntityRepository<IBackEndUserAccountEntity> BackEndUsers { get; }
        IEntityRepository<IFrontEndUserAccountEntity> FrontEndUsers { get; }
        IBackEndUserAccountEntity NewBackEndUser();
        IFrontEndUserAccountEntity NewFrontEndUser();
        IPasswordEntity NewPassword();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract(IsAbstract = true)]
    public interface IUserAccountEntity
    {
        [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Semantic.LoginName]
        string LoginName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.Validation.Length(min: 2, max: 100)]
        string FullName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.Relation.Composition]
        ICollection<IPasswordEntity> Passwords { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? LastLoginAtUtc { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Validation.MinValue(0)]
        int FailedLoginCount { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IsLockedOut { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Relation.Composition]
        ICollection<string> Claims { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract(BaseEntity = typeof(IUserAccountEntity))]
    public interface IBackEndUserAccountEntity : IUserAccountEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract(BaseEntity = typeof(IUserAccountEntity))]
    public interface IFrontEndUserAccountEntity : IUserAccountEntity
    {
        [PropertyContract.Required, PropertyContract.Semantic.EmailAddress]
        string EmailAddress { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? EmailVerifiedAtUtc { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IPasswordEntity
    {
        [PropertyContract.Required, PropertyContract.Relation.CompositionParent]
        IUserAccountEntity User { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.WriteOnly, PropertyContract.Security.Sensitive]
        string ClearText { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.SearchOnly, PropertyContract.Security.Sensitive]
        byte[] Hash { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime? ExpiresAtUtc { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool MustChange { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IUserRoleEntity : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required(AllowEmpty = false)]
        string SystemName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Relation.Composition]
        ICollection<string> DefaultClaims { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IUserActionPermissionEntity : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required(AllowEmpty = false)]
        string SystemName { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IUserDataPermissionEntity : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required(AllowEmpty = false)]
        string SystemName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Semantic.InheritorOf(typeof(IEntityAccessRule))]
        Type EntityAccessRule { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract, EntityKeyGenerator.Sequential]
    public interface IDataAuditJournalEntryEntity : IEntityPartId<long>, IEntityPartCorrelationId
    {
        [PropertyContract.Required]
        IUserAccountEntity Who { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime When { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required, PropertyContract.Validation.MaxLength(100)]
        string ModuleName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Validation.MaxLength(100)]
        string ComponentName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Validation.MaxLength(100)]
        string OperationName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string AffectedEntityName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string AffectedEntityId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string[] AffectedPropertyNames { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string[] OldPropertyValues { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string[] NewPropertyValues { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartAudit
    {
        DateTime CreatedAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        IUserAccountEntity CreatedBy { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime ModifiedAt { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        IUserAccountEntity ModifiedBy { get; set; }
    }
}
