using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    namespace Ddd
    {
        using NWheels.RestApi;

        public delegate IThisDomainObjectServices ThisDomainObjectFactory<TObject>(TObject obj);

        public interface IThisDomainObjectServices
        {
            TContext GetContext<TContext>() where TContext : class;
            string DisplayStringFormatPattern { get; }
        }

        public interface IInjectorFactory
        {
            Injector Create<TTarget>();
            Injector<T1> Create<TTarget, T1>();
            Injector<T1, T2> Create<TTarget, T1, T2>();
        }

        public struct Injector
        {
            public void Inject<TTarget>(TTarget target, out IThisDomainObjectServices thisObject)
            {
                thisObject = null;
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId)
            {
                entityId = default(TId);
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId, out IThisDomainObjectServices thisObject)
            {
                thisObject = null;
                entityId = default(TId);
            }
            public IInjectorFactory Factory => null;
        }

        public struct Injector<T1>
        {
            public void Inject<TTarget>(TTarget target, out IThisDomainObjectServices thisObject, out T1 service1)
            {
                thisObject = null;
                service1 = default(T1);
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId, out IThisDomainObjectServices thisObject, out T1 service1)
            {
                thisObject = null;
                entityId = default(TId);
                service1 = default(T1);
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId, out T1 service1)
            {
                entityId = default(TId);
                service1 = default(T1);
            }
            public void Inject(out T1 service1)
            {
                service1 = default(T1);
            }
            public IInjectorFactory Factory => null;
        }
        public struct Injector<T1, T2>
        {
            public void Inject<TTarget>(TTarget target, out IThisDomainObjectServices thisObject, out T1 service1, out T2 service2)
            {
                thisObject = null;
                service1 = default(T1);
                service2 = default(T2);
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId, out IThisDomainObjectServices thisObject, out T1 service1, out T2 service2)
            {
                thisObject = null;
                entityId = default(TId);
                service1 = default(T1);
                service2 = default(T2);
            }
            public void Inject<TTarget, TId>(TTarget target, out TId entityId, out T1 service1, out T2 service2)
            {
                entityId = default(TId);
                service1 = default(T1);
                service2 = default(T2);
            }
            public void Inject(out T1 service1, out T2 service2)
            {
                service1 = default(T1);
                service2 = default(T2);
            }
            public IInjectorFactory Factory => null;
        }

        public interface IDomainObjectValidator
        {
            void Report<TDomainObject>(
                Expression<Func<TDomainObject, object>> member,
                ValidationErrorType errorType,
                string errorMessage);
        }

        public interface IDomainObjectValidator<T> : IDomainObjectValidator
        {
            void InvalidValue(Expression<Func<T, object>> member, string message = null);
            void NullValue(Expression<Func<T, object>> member, string message = null);
            void EmptyValue(Expression<Func<T, object>> member, string message = null);
            void ValueOutOfRange(Expression<Func<T, object>> member, string message = null);
            void ValueDoesNotMatchPattern(Expression<Func<T, object>> member, string message = null);
        }

        [Serializable]
        public class DomainValidationException : Exception
        {
            public DomainValidationException(string message) : base(message)
            {
            }
        }

        [Serializable]
        public class DomainValidationException<TObject> : Exception
        {
            public DomainValidationException(TObject obj, string message) : base(message)
            {
            }
        }

        [Serializable]
        public class BrokenInvariantDomainException : Exception
        {
            public BrokenInvariantDomainException(string message) : base(message)
            {
            }
        }

        [Serializable]
        public class InvalidRequestDomainException : Exception
        {
            public InvalidRequestDomainException(string message) : base(message)
            {
            }
        }

        public enum ValidationErrorType
        {
            NotSpecified,
            ValueIsNull,
            ValueIsEmpty,
            ValueIsOutOfRange,
            ValueDoesNotMatchPattern,
            ValueIsInvalid
        }

        public static class EntityRef<TEntity>
            where TEntity : class
        {
            public static EntityRef<TId, TEntity> ToId<TId>(TId id)
            {
                return new EntityRef<TId, TEntity>(id);
            }
        }

        public struct ValueObject<T>
        {
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public T Value { get; set; }
        }

        public struct EntityRef<TId, TEntity>
            where TEntity : class
        {
            public EntityRef(TId id)
            {
                this.Id = id;
                this.IsLoaded = false;
                this.CanLoad = false;
                this.Entity = null;
            }

            public EntityRef(TEntity entity)
            {
                this.Id = default(TId);//??
                this.IsLoaded = true;
                this.CanLoad = true;
                this.Entity = entity;
            }

            public TId Id { get; }
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public TEntity Entity { get; }

            public static EntityRef<TId, TEntity> To(TEntity entity)
            {
                return new EntityRef<TId, TEntity>(entity);
            }
        }

        public struct EntitySet<TId, TEntity> : IQueryable<TEntity>
            where TEntity : class
        {
            private Type _elementType;
            private Expression _expression;
            private IQueryProvider _provider;
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public ISet<EntityRef<TId, TEntity>> Set { get; }

            IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            Type IQueryable.ElementType
            {
                get { return _elementType; }
            }

            Expression IQueryable.Expression
            {
                get { return _expression; }
            }

            IQueryProvider IQueryable.Provider
            {
                get { return _provider; }
            }
        }

        public struct ValueObjectList<TValueObject>
        {
            public bool IsLoaded { get; }
            public bool CanLoad { get; }
            public IList<TValueObject> List { get; }
        }

        public static class TypeContract
        {
            public class BoundedContextAttribute : Attribute
            {
            }
        }

        public static class ResourceCatalogBuilderExtensions
        {
            public static ResourceCatalogBuilder AddDomainTransaction<TContext>(
                this ResourceCatalogBuilder catalogBuilder,
                Expression<Func<TContext, Task>> tx)
            {
                return catalogBuilder;
            }

            public static ResourceCatalogBuilder AddDomainRepository<TContext, TAggregate>(this ResourceCatalogBuilder catalogBuilder)
            {
                return catalogBuilder;
            }
        }
    }
}
