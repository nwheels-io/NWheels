using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EntityObjectFactory : ConventionObjectFactory
    {
        public EntityObjectFactory(DynamicModule module)
            : base(module, new EntityObjectConvention())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T NewEntity<T>() where T : class
        {
            return CreateInstanceOf<T>().UsingDefaultConstructor();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntityObjectConvention : ImplementationConvention
        {
            public EntityObjectConvention()
                : base(Will.ImplementPrimaryInterface)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TT.TInterface> writer)
            {
                writer.ImplementInterfaceVirtual<TT.TInterface>().AllProperties().ImplementAutomatic();

                writer.Constructor(cw => writer.AllProperties(p => IsGenericICollection(p.PropertyType)).ForEach(p => {
                    using ( TT.CreateScope<TT.TProperty, TT.TItem>(p.PropertyType, p.PropertyType.GetGenericArguments()[0]) )
                    {
                        writer.OwnerClass.GetPropertyBackingField(p).AsOperand<TT.TProperty>().Assign(
                            cw.New<HashSet<TT.TItem>>().CastTo<TT.TProperty>());
                    }
                }));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsGenericICollection(Type type)
            {
                return (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(ICollection<>));
            }
        }
    }
}
