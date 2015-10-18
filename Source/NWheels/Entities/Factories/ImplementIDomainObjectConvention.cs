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
            ImplementTemporaryKey(domainObjectImplementation);
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

        private void ImplementTemporaryKey(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Property(intf => intf.TemporaryKey).ImplementAutomatic();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementBeforeAfterCommit(ImplementationClassWriter<IDomainObject> writer)
        {
            MethodInfo[] beforeSave;
            MethodInfo[] afterSave;
            MethodInfo[] beforeDelete;
            MethodInfo[] afterDelete;
            TryFindEntityTriggerMethods(out beforeSave, out afterSave, out beforeDelete, out afterDelete);

            writer
                .Method(x => x.BeforeCommit).Implement(w => {
                    if ( HaveTriggerMethods(beforeSave) )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                            WriteTriggerMethodCalls(w, beforeSave);
                        });
                    }
                    if ( HaveTriggerMethods(beforeDelete) )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                            WriteTriggerMethodCalls(w, beforeDelete);
                        });
                    }
                })
                .Method(x => x.AfterCommit).Implement(w => {
                    if ( HaveTriggerMethods(afterSave) )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                            WriteTriggerMethodCalls(w, afterSave);
                        });
                    }
                    if ( HaveTriggerMethods(afterDelete) )
                    {
                        w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                            WriteTriggerMethodCalls(w, afterDelete);
                        });
                    }
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool HaveTriggerMethods(MethodInfo[] methods)
        {
            return (methods != null && methods.Length > 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteTriggerMethodCalls(MethodWriterBase writer, MethodInfo[] methods)
        {
            var w = writer;

            if ( methods != null )
            {
                foreach ( var method in methods )
                {
                    w.This<TT.TBase>().Void(method);
                }
            }
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
            out MethodInfo[] beforeSave, 
            out MethodInfo[] afterSave, 
            out MethodInfo[] beforeDelete, 
            out MethodInfo[] afterDelete)
        {
            var members = TypeMemberCache.Of(_context.BaseContext.BaseType);

            beforeSave = TryFindTriggerMethods(members, TriggerMethodNameBeforeSave);
            afterSave = TryFindTriggerMethods(members, TriggerMethodNameAfterSave);
            beforeDelete = TryFindTriggerMethods(members, TriggerMethodNameBeforeDelete);
            afterDelete = TryFindTriggerMethods(members, TriggerMethodNameAfterDelete);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MethodInfo[] TryFindTriggerMethods(TypeMemberCache members, string methodName)
        {
            return members
                .SelectVoids(m => m.Name == methodName && !m.IsPublic && m.GetParameters().Length == 0)
                .ToArray();
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