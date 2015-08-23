using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIDomainObjectConvention : ImplementationConvention
    {
        public const string MethodNameOnEntityCommitting = "EntityTriggerBeforeSave";
        public const string MethodNameOnEntityCommitted = "EntityTriggerAfterSave";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly DomainObjectFactoryContext _context;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIDomainObjectConvention(DomainObjectFactoryContext context)
            : base(Will.ImplementBaseClass)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
        {
            ImplementIsModified(writer);

            var domainObjectImplementation = writer.ImplementInterfaceExplicitly<IDomainObject>();
            ImplementState(domainObjectImplementation);
            ImplementCommittingCommitted(domainObjectImplementation);
            ImplementValidate(domainObjectImplementation);
            
            ImplementGetContainedObject(writer);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementIsModified(ImplementationClassWriter<TT.TBase> writer)
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementState(ImplementationClassWriter<IDomainObject> writer)
        {
            writer
                .Property(intf => intf.State).Implement(p => 
                    p.Get(gw => {
                        gw.Return(gw.Iif(
                            gw.This<IObject>().Prop<bool>(x => x.IsModified),
                            Static.Func(EntityStateExtensions.SetModified, _context.EntityStateField),
                            _context.EntityStateField));
                    })
                );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementCommittingCommitted(ImplementationClassWriter<IDomainObject> writer)
        {
            MethodInfo committingCallback;
            MethodInfo committedCallback;
            TryFindCommitCallbackMethods(out committingCallback, out committedCallback);

            writer
                .Method(x => x.BeforeSave).Implement(w => {
                    if ( committingCallback != null )
                    {
                        w.This<TT.TBase>().Void(committingCallback);
                    }
                })
                .Method(x => x.AfterSave).Implement(w => {
                    if ( committedCallback != null )
                    {
                        w.This<TT.TBase>().Void(committedCallback);
                    }
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementValidate(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method(x => x.Validate).Implement(w => {
                _context.PropertyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteValidation(w);
                    });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementGetContainedObject(ImplementationClassWriter<TT.TBase> writer)
        {
            writer.ImplementInterfaceExplicitly<IContain<IPersistableObject>>()
                .Method<IPersistableObject>(intf => intf.GetContainedObject).Implement(w =>
                    w.Return(_context.PersistableObjectField.CastTo<IPersistableObject>())
                );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryFindCommitCallbackMethods(out MethodInfo committingCallback, out MethodInfo committedCallback)
        {
            committingCallback = null;
            committedCallback = null;

            if ( _context.MetaType.DomainObjectType != null )
            {
                committingCallback =
                    _context.DomainObjectMembers.SelectVoids(m => m.Name == MethodNameOnEntityCommitting && !m.IsPublic && m.GetParameters().Length == 0)
                    .ToArray()
                    .FirstOrDefault();

                committedCallback =
                    _context.DomainObjectMembers.SelectVoids(m => m.Name == MethodNameOnEntityCommitted && !m.IsPublic && m.GetParameters().Length == 0)
                    .ToArray()
                    .FirstOrDefault();
            }
        }
    }
}