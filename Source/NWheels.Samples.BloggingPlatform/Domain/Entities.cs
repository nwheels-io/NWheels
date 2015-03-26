using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Modules.Auth;

namespace NWheels.Samples.BloggingPlatform.Domain
{
    public interface IBlogDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IUserAccountEntity> AllUsers { get; }
        IEntityRepository<IAuthorEntity> Authors { get; }
        IEntityRepository<IArticleEntity> Articles { get; }
        IEntityRepository<IPostEntity> Posts { get; }
        IEntityRepository<ITagEntity> Tags { get; }
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
    public interface IAuthorEntity : IUserAccountEntity
    {
        ICollection<ITopLevelContentEntity> AuthoredContents { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IAbstractContentEntity : IEntityPartId<int>, IEntityPartAudit
    {
        string Markdown { get; set; }
        ICollection<ITagEntity> Tags { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface ITopLevelContentEntity : IAbstractContentEntity
    {
        string Title { get; set; }
        ICollection<IReplyEntity> Replies { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IArticleEntity : ITopLevelContentEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IPostEntity : ITopLevelContentEntity
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
        string Name { get; set; }
        string Description { get; set; }
        ICollection<IAbstractContentEntity> RelatedContents { get; }
    }
}
