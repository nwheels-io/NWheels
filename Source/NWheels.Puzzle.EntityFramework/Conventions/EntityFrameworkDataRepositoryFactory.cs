using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Puzzle.EntityFramework.Impl;
using System.Reflection;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EntityFrameworkDataRepositoryFactory : ConventionObjectFactory
    {
        public EntityFrameworkDataRepositoryFactory(DynamicModule module, EntityFrameworkEntityObjectFactory entityFactory)
            : base(module, new DataRepositoryConvention(entityFactory))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DataRepositoryConvention : ImplementationConvention
        {
            private readonly EntityFrameworkEntityObjectFactory _entityFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DataRepositoryConvention(EntityFrameworkEntityObjectFactory entityFactory)
                : base(Will.InspectDeclaration | Will.ImplementPrimaryInterface)
            {
                _entityFactory = entityFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(EntityFrameworkDataRepositoryBase);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                var initializers = new List<Action<ConstructorWriter>>();
                var entityTypesInRepository = new List<Type>();

                writer.AllProperties(MustImplementProperty).ForEach(property => {
                    Type entityContractType;
                    Type entityImplementationType;
                    ValidateContractProperty(property, out entityContractType, out entityImplementationType);

                    writer.Property(property).Implement(p => p.Get(w => w.Return(p.BackingField)));

                    initializers.Add(cw => {
                        using ( TT.CreateScope<TT.TContract, TT.TImpl>(entityContractType, entityImplementationType) )
                        {
                            writer.OwnerClass.GetPropertyBackingField(property)
                                .AsOperand<IEntityRepository<TT.TContract>>()
                                .Assign(cw.New<EntityFrameworkEntityRepository<TT.TContract, TT.TImpl>>(cw.This<EntityFrameworkDataRepositoryBase>()));
                        }
                    });

                    entityTypesInRepository.Add(entityImplementationType);
                });

                writer.Constructor<DbConnection, bool>((cw, connection, autoCommit) => {
                    cw.Base(cw.Const<DbCompiledModel>(null), connection, autoCommit);
                    initializers.ForEach(init => init(cw));
                });

                writer.ImplementBase<EntityFrameworkDataRepositoryBase>().Method<IEnumerable<Type>>(x => x.GetEntityTypesInRepository).Implement(m => 
                    m.Return(m.NewArray<Type>(constantValues: entityTypesInRepository.ToArray()))
                );
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool MustImplementProperty(PropertyInfo property)
            {
                return (property.DeclaringType != typeof(IApplicationDataRepository) && property.DeclaringType != typeof(IUnitOfWork));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateContractProperty(PropertyInfo property, out Type entityContractType, out Type entityImplementationType)
            {
                var type = property.PropertyType;

                if ( !type.IsGenericType || type.IsGenericTypeDefinition || type.GetGenericTypeDefinition() != typeof(IEntityRepository<>) )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "Property must be of type IEntityRepository<T>");
                }

                if ( property.GetGetMethod() == null || property.GetSetMethod() != null )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "Property must be read-only");
                }

                entityContractType = property.PropertyType.GetGenericArguments()[0];

                if ( !EntityContractAttribute.IsEntityContract(entityContractType) )
                {
                    throw new ContractConventionException(
                        this, property.DeclaringType, property, "IEntityRepository<T> must specify T which is an entity contract interface");
                }

                entityImplementationType = _entityFactory.GetOrBuildEntityImplementation(entityContractType);
            }
        }
    }
}
