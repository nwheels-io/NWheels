using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using IR1 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository1;
using IR2 = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository2;
using IR3A = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository3A;
using IR3B = NWheels.Puzzle.EntityFramework.ComponentTests.Interfaces.Repository3B;

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
                [PropertyContract.Storage.RelationalMapping(Column = "Id")]
                int Id { get; set; }
                [PropertyContract.Storage.RelationalMapping(Column = "Name")]
                string Name { get; set; }
                [PropertyContract.Storage.RelationalMapping(Column = "Price")]
                decimal Price { get; set; }
            }
            [EntityContract]
            public interface IOrder
            {
                int Id { get; set; }
                DateTime PlacedAt { get; set; }
                ICollection<IOrderLine> OrderLines { get; }
                [PropertyContract.DefaultValue(OrderStatus.New)]
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
                IEntityRepository<IR3A.IUserAccountEntity> AllUsers { get; }
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
            public interface IAuthorEntity : IR3A.IUserAccountEntity
            {
                ICollection<ITopLevelContentEntity> AuthoredContents { get; }
            }

            [EntityContract]
            public interface IAbstractContentEntity : IEntityPartId<int>, IR3A.IEntityPartAudit
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

        public static class Repository3A
        {
            [EntityContract]
            [MustHaveMixin(typeof(IEntityPartId<>))]
            public interface IUserAccountEntity
            {
                [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Validation.Length(4, 50)]
                string LoginName { get; set; }
                
                [PropertyContract.Validation.MaxLength(50)]
                string NickName { get; set; }

                [PropertyContract.Validation.MaxLength(50)]
                string FullName { get; set; }
                
                [PropertyContract.Required, PropertyContract.Semantic.EmailAddress]
                string EmailAddress { get; set; }
                
                bool IsEmailVerified { get; set; }
                
                [PropertyContract.Required]
                ICollection<IPasswordEntity> Passwords { get; }
                
                ICollection<IUserRoleEntity> Roles { get; }
                
                DateTime LastLoginAt { get; set; }
                
                [PropertyContract.Validation.MinValue(0)]
                int FailedLoginCount { get; set; }
                
                bool IsLockedOut { get; set; }
            }

            [EntityContract]
            [MustHaveMixin(typeof(IEntityPartId<>))]
            [MustHaveMixin(typeof(IEntityPartUserRole<>))]
            public interface IUserRoleEntity
            {
                [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Validation.MaxLength(50)]
                string Name { get; set; }
            }

            [EntityPartContract]
            public interface IEntityPartUserRole<TRole>
            {
                [PropertyContract.Required]
                TRole Role { get; set; }
            }

            [EntityContract]
            [MustHaveMixin(typeof(IEntityPartId<>))]
            public interface IPasswordEntity
            {
                [PropertyContract.Required]
                IUserAccountEntity User { get; set; }

                [PropertyContract.WriteOnly, PropertyContract.Security.Sensitive]
                string ClearText { get; set; }

                [PropertyContract.Required, PropertyContract.SearchOnly, PropertyContract.Security.Sensitive]
                byte[] Hash { get; set; }

                DateTime Expiration { get; set; }

                bool MustChange { get; set; }
            }
            
            [EntityPartContract]
            public interface IEntityPartAudit
            {
                [PropertyContract.Validation.Past("00:00:00")]
                DateTime CreatedAt { get; set; }

                [PropertyContract.Required]
                IUserAccountEntity CreatedBy { get; set; }

                [PropertyContract.Validation.Past("00:00:00")]
                DateTime ModifiedAt { get; set; }

                [PropertyContract.Required]
                IUserAccountEntity ModifiedBy { get; set; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Repository3B
        {
            public enum MyAppUserRole
            {
                MyGuest = 0,
                MyPower = 1,
                MyAdmin = 2
            }

            public interface IMyAppUserAccountEntity : IR3A.IUserAccountEntity, IEntityPartId<int>
            {
                new ICollection<IMyAppUserRoleEntity> Roles { get; }
            }

            public interface IMyAppUserRoleEntity : IR3A.IUserRoleEntity, IEntityPartId<int>, IR3A.IEntityPartUserRole<MyAppUserRole>
            {
            }

            public interface IMyAppDataRepository : IApplicationDataRepository
            {
                IEntityRepository<IMyAppUserAccountEntity> Users { get; }
            }
        }
    }
}
