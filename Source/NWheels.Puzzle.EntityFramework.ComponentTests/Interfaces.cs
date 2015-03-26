using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using IR1 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository1;
using IR2 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository2;
using IR3 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository3;


namespace NWheels.Puzzle.EntityFramework.ComponentTests
{
    public static class Interfaces
    {
        public static class Repository1
        {
            public enum OrderStatus
            {
                New = 1,
                PaymentReceived = 2,
                ProductsShipped = 3
            }
            public interface IOnlineStoreRepository : IApplicationDataRepository
            {
                IOrderLine NewOrderLine(IOrder order, IProduct product, int quantity);
                IEntityRepository<IProduct> Products { get; }
                IEntityRepository<IOrder> Orders { get; }
            }
            [EntityContract]
            public interface IProduct
            {
                int Id { get; set; }
                string Name { get; set; }
                decimal Price { get; set; }
            }
            [EntityContract]
            public interface IOrder
            {
                int Id { get; set; }
                DateTime PlacedAt { get; set; }
                ICollection<IOrderLine> OrderLines { get; }
                [DefaultValue(OrderStatus.New)]
                OrderStatus Status { get; set; }
            }
            [EntityContract]
            public interface IOrderLine
            {
                int Id { get; set; }
                IOrder Order { get; set; }
                IProduct Product { get; set; }
                int Quantity { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Repository2
        {
            public interface IBlogDataRepository : IApplicationDataRepository
            {
                IEntityRepository<IR3.IUserAccountEntity> AllUsers { get; }
                IEntityRepository<IAuthorEntity> Authors { get; }
                IEntityRepository<IArticleEntity> Articles { get; }
                IEntityRepository<IPostEntity> Posts { get; }
                IEntityRepository<ITagEntity> Tags { get; }
            }

            public enum UserRole
            {
                Reader = 0,
                Author = 1,
                Admin = 2
            }

            public enum ReplyStatus
            {
                Submitted,
                Accepted,
                Rejected
            }

            [EntityContract]
            public interface IAuthorEntity : IR3.IUserAccountEntity
            {
                ICollection<ITopLevelContentEntity> AuthoredContents { get; }
            }

            [EntityContract]
            public interface IAbstractContentEntity : IEntityPartId<int>, IR3.IEntityPartAudit
            {
                string Markdown { get; set; }
                ICollection<ITagEntity> Tags { get; }
            }

            [EntityContract]
            public interface ITopLevelContentEntity : IAbstractContentEntity
            {
                ICollection<IReplyEntity> Replies { get; }
            }

            [EntityContract]
            public interface IArticleEntity : ITopLevelContentEntity
            {
                string Title { get; set; }
            }

            [EntityContract]
            public interface IPostEntity : ITopLevelContentEntity
            {
            }

            [EntityContract]
            public interface IReplyEntity : IAbstractContentEntity
            {
                ReplyStatus Status { get; set; }
            }

            [EntityContract]
            public interface ITagEntity : IEntityPartId<int>
            {
                string Name { get; set; }
                string Description { get; set; }
                ICollection<IAbstractContentEntity> RelatedContents { get; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Repository3
        {
            public enum MyUserRole
            {
                MyGuest = 0,
                MyPower = 1,
                MyAdmin = 2
            }
            [EntityContract]
            public interface IUserAccountEntity
            {
                string LoginName { get; set; }
                string NickName { get; set; }
                string FullName { get; set; }
                string EmailAddress { get; set; }
                bool IsEmailVerified { get; set; }
                IPasswordEntity CurrentPassword { get; set; }
                ICollection<IPasswordEntity> PasswordHistory { get; }
                ICollection<IUserRoleEntity> Roles { get; }
                DateTime LastLoginAt { get; set; }
                int FailedLoginCount { get; set; }
                bool IsLockedOut { get; set; }
            }
            [EntityContract]
            public interface IUserRoleEntity
            {
                string Name { get; set; }
            }
            [EntityPartContract]
            public interface IEntityPartUserRoleId<TRoleId>
            {
                TRoleId RoleId { get; set; }
            }
            [EntityContract]
            public interface IPasswordEntity
            {
                IUserAccountEntity User { get; set; }
                byte[] Hash { get; set; }
                DateTime Expiration { get; set; }
                bool MustChange { get; set; }
            }
            [EntityPartContract]
            public interface IEntityPartAudit
            {
                DateTime CreatedAt { get; set; }
                [Required]
                IUserAccountEntity CreatedBy { get; set; }
                DateTime ModifiedAt { get; set; }
                [Required]
                IUserAccountEntity ModifiedBy { get; set; }
            }
            public interface IMyDataRepository : IApplicationDataRepository
            {
                IEntityRepository<IUserAccountEntity> UserAccounts { get; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
