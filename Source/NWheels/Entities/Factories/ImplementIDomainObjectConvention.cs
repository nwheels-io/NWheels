using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.TypeModel.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIDomainObjectConvention : ImplementationConvention
    {
        public const string TriggerMethodNameBeforeSave = "EntityTriggerBeforeSave";
        public const string TriggerMethodNameAfterSave = "EntityTriggerAfterSave";
        public const string TriggerMethodNameBeforeDelete = "EntityTriggerBeforeDelete";
        public const string TriggerMethodNameAfterDelete = "EntityTriggerAfterDelete";
        public const string TriggerMethodNameInsteadOfSave = "EntityTriggerInsteadOfSave";
        public const string TriggerMethodNameInsteadOfDelete = "EntityTriggerInsteadOfDelete";

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
            ImplementBeforeAfterCommit(domainObjectImplementation);
            ImplementValidate(domainObjectImplementation);
            
            ImplementGetContainedObject(writer);
            ImplementToString(writer);
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

        private void ImplementBeforeAfterCommit(ImplementationClassWriter<IDomainObject> writer)
        {
            MethodInfo beforeSave;
            MethodInfo afterSave;
            MethodInfo beforeDelete;
            MethodInfo afterDelete;
            TryFindEntityTriggerMethods(out beforeSave, out afterSave, out beforeDelete, out afterDelete);

            writer
                .Method(x => x.BeforeCommit).Implement(w => {
                    if ( beforeSave != null )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                            w.This<TT.TBase>().Void(beforeSave);
                        });
                    }
                    if ( beforeDelete != null )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                            w.This<TT.TBase>().Void(beforeDelete);
                        });
                    }
                })
                .Method(x => x.AfterCommit).Implement(w => {
                    if ( afterSave != null )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                            w.This<TT.TBase>().Void(afterSave);
                        });
                    }
                    if ( afterDelete != null )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                            w.This<TT.TBase>().Void(afterDelete);
                        });
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

        private void TryFindEntityTriggerMethods(
            out MethodInfo beforeSave, 
            out MethodInfo afterSave, 
            out MethodInfo beforeDelete, 
            out MethodInfo afterDelete)
        {
            var members = TypeMemberCache.Of(_context.BaseContext.BaseType);

            beforeSave = TryFindEntityTriggerMethod(members, TriggerMethodNameBeforeSave);
            afterSave = TryFindEntityTriggerMethod(members, TriggerMethodNameAfterSave);
            beforeDelete = TryFindEntityTriggerMethod(members, TriggerMethodNameBeforeDelete);
            afterDelete = TryFindEntityTriggerMethod(members, TriggerMethodNameAfterDelete);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MethodInfo TryFindEntityTriggerMethod(TypeMemberCache members, string methodName)
        {
            return members
                .SelectVoids(m => m.Name == methodName && !m.IsPublic && m.GetParameters().Length == 0)
                .ToArray()
                .FirstOrDefault();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementToString(ImplementationClassWriter<TT.TBase> writer)
        {
            if ( _context.MetaType.BaseType == null )
            {
                writer.Method<string>(x => x.ToString).Implement(w => {
                    w.Return(_context.PersistableObjectField.FuncToString());
                });
            }
        }
    }
}