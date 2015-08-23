using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class DomainObjectWiredPropertiesConvention : ImplementationConvention
    {
        public const string EntityStateWiredPropertyName = "EntityState";
        public const string WasModifiedWiredPropertySuffix = "WasModified";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectWiredPropertiesConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_context.MetaType.DomainObjectType != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
        {
            ImplementEntityStateProperty(writer);
            ImplementWasModifiedProperties(writer);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementEntityStateProperty(ImplementationClassWriter<TT.TBase> writer)
        {
            writer.Properties<EntityState>(@where: IsEntityStateProperty).Implement(p => 
                p.Get(gw => {
                    gw.Return(gw.This<IDomainObject>().Prop<EntityState>(x => x.State));
                })
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementWasModifiedProperties(ImplementationClassWriter<TT.TBase> writer)
        {
            writer.Properties<bool>(@where: IsWasModifiedWiredProperty).Implement(p => 
                p.Get(gw => {
                    var associatedEntityProperty = _context.MetaType.GetPropertyByName(p.OwnerProperty.Name.TrimTail(WasModifiedWiredPropertySuffix));
                    var entityPropertyStrategy = _context.PropertyMap[associatedEntityProperty];

                    using ( TT.CreateScope<TT.TProperty>(associatedEntityProperty.ClrType) )
                    {
                        entityPropertyStrategy.WriteReturnTrueIfModified(gw);

                        gw.If(_context.ModifiedVector.WriteNonZeroBitTest(gw, associatedEntityProperty.PropertyIndex)).Then(() => {
                            gw.Return(gw.Const(true));
                        });
                    }

                    gw.Return(false);
                })
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsEntityStateProperty(PropertyInfo p)
        {
            return (p.Name == EntityStateWiredPropertyName && p.CanRead && !p.CanWrite && p.GetMethod.IsFamily && p.GetMethod.IsAbstract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsWasModifiedWiredProperty(PropertyInfo p)
        {
            if ( p.Name == WasModifiedWiredPropertySuffix || !p.Name.EndsWith(WasModifiedWiredPropertySuffix) )
            {
                return false;
            }

            var associatedEntityPropertyName = p.Name.TrimTail(WasModifiedWiredPropertySuffix);
            IPropertyMetadata metaProperty;

            if ( !_context.MetaType.TryGetPropertyByName(associatedEntityPropertyName, out metaProperty) )
            {
                return false;
            }

            return (p.CanRead && !p.CanWrite && p.GetMethod.IsFamily && p.GetMethod.IsAbstract);
        }
    }
}
