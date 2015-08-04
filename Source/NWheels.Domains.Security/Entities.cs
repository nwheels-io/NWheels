using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.Security
{
    public interface IUserAccountDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IUserAccountEntity> AllUsers { get; }
        //IEntityRepository<IBackEndUserAccountEntity> BackEndUsers { get; }
        //IEntityRepository<IFrontEndUserAccountEntity> FrontEndUsers { get; }
        IEntityRepository<IUserRoleEntity> UserRoles { get; }
        IEntityRepository<IOperationPermissionEntity> OperationPermissions { get; }
        IEntityRepository<IEntityAccessRuleEntity> EntityAccessRules { get; }
        IPasswordEntity NewPassword();
        IBackEndUserAccountEntity NewBackEndUser();
        IFrontEndUserAccountEntity NewFrontEndUser();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract(IsAbstract = true)]
    public interface IUserAccountEntity : IEntityPartClaimsContainer, IEntityPartId<int>
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
    public interface IPasswordEntity : IEntityPartId<int>
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
    
    public static partial class EntityExtensions
    {
        public static bool IsExpired(this IPasswordEntity password, DateTime utcNow)
        {
            if ( !password.ExpiresAtUtc.HasValue )
            {
                return false;
            }

            return (utcNow >= password.ExpiresAtUtc.Value);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartClaim
    {
        [PropertyContract.Required(AllowEmpty = false)]
        string ClaimValueType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required(AllowEmpty = false)]
        string ClaimValue { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IEntityPartClaimsContainer
    {
        [PropertyContract.Relation.Composition, PropertyContract.Storage.Json]
        string[] AssociatedRoles { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Relation.Composition, PropertyContract.Storage.Json]
        string[] AssociatedPermissions { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Relation.Composition, PropertyContract.Storage.Json]
        string[] AssociatedDataRules { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IUserRoleEntity : IEntityPartUniqueDisplayName, IEntityPartClaim, IEntityPartClaimsContainer, IEntityPartId<int>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IOperationPermissionEntity : IEntityPartUniqueDisplayName, IEntityPartClaim, IEntityPartId<int>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IEntityAccessRuleEntity : IEntityPartUniqueDisplayName, IEntityPartClaim, IEntityPartId<int>
    {
        [PropertyContract.Semantic.InheritorOf(typeof(IEntityAccessRule)), PropertyContract.Storage.ClrType]
        Type RuleObject { get; set; }
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
