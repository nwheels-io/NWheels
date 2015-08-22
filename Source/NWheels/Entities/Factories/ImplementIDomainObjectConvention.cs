using Hapil;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;

namespace NWheels.Entities.Factories
{
    public class ImplementIDomainObjectConvention : ImplementationConvention
    {
        private readonly DomainObjectFactoryContext _context;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIDomainObjectConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {

            writer.ImplementInterfaceExplicitly<IObject>()
                .Property(intf => intf.IsModified).Implement(p =>
                    p.Get(gw => {
                        gw.If(_context.ModifiedVector.WriteNonZeroTest(gw)).Then(() => {
                            gw.Return(gw.Const(true));        
                        });

                        _context.PropertyMap.InvokeStrategies(
                            strategy => {
                                strategy.WriteReturnTrueIfModified(gw);
                            });

                        gw.Return(false);
                    })
                );

            writer.ImplementInterfaceExplicitly<IDomainObject>()
                .Property(intf => intf.State).Implement(p => 
                    p.Get(gw => {
                        gw.Return(gw.Iif(
                            gw.This<IObject>().Prop<bool>(x => x.IsModified),
                            Static.Func(EntityStateExtensions.SetModified, _context.EntityStateField),
                            _context.EntityStateField));
                    })
                );

            writer.ImplementInterfaceExplicitly<IContain<IPersistableObject>>()
                .Method<IPersistableObject>(intf => intf.GetContainedObject).Implement(w =>
                    w.Return(_context.PersistableObjectField.CastTo<IPersistableObject>())
                );
        }

        #endregion
    }
}