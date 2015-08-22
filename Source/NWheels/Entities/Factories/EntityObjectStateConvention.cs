using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;

namespace NWheels.Entities.Factories
{
    public class EntityObjectStateConvention : CompositeConvention
    {
        public EntityObjectStateConvention()
            : this(new ConventionContext())
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EntityObjectStateConvention(ConventionContext context) 
            : base(
                new StatePropertyConvention(context), 
                new StateInitializationConvention(context))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConventionContext
        {
            public Field<EntityState> EntityStateField { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StatePropertyConvention : ImplementationConvention
        {
            private readonly ConventionContext _context;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StatePropertyConvention(ConventionContext context)
                : base(Will.ImplementBaseClass)
            {
                _context = context;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                _context.EntityStateField = writer.Field<EntityState>("$entityState");

                writer.ImplementInterfaceExplicitly<IEntityObjectBase>()
                    .Property(intf => intf.State).Implement(p =>
                        p.Get(gw => gw.Return(_context.EntityStateField))
                    );

                writer.ImplementInterfaceExplicitly<IObject>()
                    .Property(intf => intf.IsModified).Implement(p =>
                        p.Get(gw => gw.Return(Static.Func(EntityStateExtensions.IsModified, _context.EntityStateField)))
                    );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StateInitializationConvention : DecorationConvention
        {
            private readonly ConventionContext _context;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StateInitializationConvention(ConventionContext context)
                : base(Will.DecorateConstructors)
            {
                _context = context;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnConstructor(MethodMember member, Func<ConstructorDecorationBuilder> decorate)
            {
                if ( IsInitializationConstructor(member) )
                {
                    decorate().OnSuccess(w => _context.EntityStateField.Assign(EntityState.NewPristine));
                }
                else
                {
                    decorate().OnSuccess(w => _context.EntityStateField.Assign(EntityState.RetrievedPristine));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static bool IsInitializationConstructor(MethodMember member)
            {
                return member.Signature.ArgumentCount > 0;
            }
        }
    }
}
