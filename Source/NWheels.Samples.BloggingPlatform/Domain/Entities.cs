using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Modules.Security;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public interface IBlogDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IBlogEntity> Blogs { get; }
        IEntityRepository<IBlogUserAccountEntity> Users { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BlogUserRole
    {
        Guest = 0,
        Author = 1,
        Admin = 2
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum BlogReplyStatus
    {
        Submitted,
        Accepted,
        Rejected
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IBlogEntity : IEntityPartId<int>
    {
        IEntityRepository<IArticleEntity> Articles { get; }
        IEntityRepository<IPostEntity> Posts { get; }
        IEntityRepository<ITagEntity> Tags { get; }
        IEntityRepository<IBlogUserAuthorizationEntity> Authorizations { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IBlogUserAuthorizationEntity :
        IEntityPartId<int>
    {
        [PropertyContract.Required]
        IBlogEntity Blog { get; set; }
        
        [PropertyContract.Required]
        IBlogUserAccountEntity User { get; set; }
        
        [PropertyContract.Required]
        IBlogUserRoleEntity Role { get; set; } 
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IBlogUserAccountEntity : 
        IUserAccountEntity, 
        IEntityPartId<int>
    {
        ICollection<IBlogUserAuthorizationEntity> Authorizations { get; }
        ICollection<IMainContentEntity> AuthoredContents { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IBlogUserRoleEntity : 
        IUserRoleEntity<BlogUserRole>, 
        IEntityPartId<int>
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IAbstractContentEntity : 
        IEntityPartId<int>, 
        IEntityPartAudit
    {
        [PropertyContract.Required]
        IBlogUserAccountEntity Author { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [PropertyContract.Required]
        string Markdown { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ICollection<ITagEntity> Tags { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IMainContentEntity : IAbstractContentEntity
    {
        string Title { get; set; }
        ICollection<IReplyEntity> Replies { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IArticleEntity : IMainContentEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IPostEntity : IMainContentEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IReplyEntity : IAbstractContentEntity
    {
        BlogReplyStatus Status { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface ITagEntity : IEntityPartId<int>
    {
        [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Validation.Length(3, 50)]
        string Name { get; set; }

        [PropertyContract.Required, PropertyContract.Validation.Length(3, 100)]
        string Description { get; set; }

        [PropertyContract.Relation.ManyToMany]
        ICollection<IAbstractContentEntity> RelatedContents { get; }
    }
}
