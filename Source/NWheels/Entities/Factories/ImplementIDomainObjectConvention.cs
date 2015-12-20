using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.TypeModel.Core;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class ImplementIDomainObjectConvention : ImplementationConvention
    {
        public const string TriggerMethodNameOnNew = "EntityTriggerOnNew";
        public const string TriggerMethodNameOnValidateInvariants = "EntityTriggerOnValidateInvariants";
        public const string TriggerMethodNameBeforeSave = "EntityTriggerBeforeSave";
        public const string TriggerMethodNameAfterSave = "EntityTriggerAfterSave";
        public const string TriggerMethodNameBeforeDelete = "EntityTriggerBeforeDelete";
        public const string TriggerMethodNameAfterDelete = "EntityTriggerAfterDelete";
        public const string TriggerMethodNameInsteadOfSave = "EntityTriggerInsteadOfSave";
        public const string TriggerMethodNameInsteadOfDelete = "EntityTriggerInsteadOfDelete";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly DomainObjectFactoryContext _context;
        private Field<IComponentContext> _componentsField;
        private MethodInfo[] _onNew;
        private MethodInfo[] _onValidate;
        private MethodInfo[] _beforeSave;
        private MethodInfo[] _afterSave;
        private MethodInfo[] _beforeDelete;
        private MethodInfo[] _afterDelete;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImplementIDomainObjectConvention(DomainObjectFactoryContext context)
            : base(Will.InspectDeclaration | Will.ImplementBaseClass)
        {
            _context = context;
            _context.OnWriteOnNewTriggerCalls = WriteOnNewTriggerMethodCalls;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnInspectDeclaration(ObjectFactoryContext context)
        {
            TryFindEntityTriggerMethods(context.BaseType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
        {
            _componentsField = writer.DependencyField<IComponentContext>("$components");
            _context.ThisLazyLoaderField = writer.Field<IPersistableObjectLazyLoader>("$thisLazyLoader");

            ImplementIsModified(writer);

            var domainObjectImplementation = writer.ImplementInterfaceExplicitly<IDomainObject>();

            ImplementState(domainObjectImplementation);
            ImplementBeforeCommit(domainObjectImplementation);
            ImplementAfterCommit(domainObjectImplementation);
            ImplementValidate(domainObjectImplementation);

            ImplementExportValues(domainObjectImplementation);
            ImplementImportValues(domainObjectImplementation);
            ImplementInitializeValues(domainObjectImplementation);
            ImplementSetLazyLoader(domainObjectImplementation);

            ImplementEntityId(domainObjectImplementation);
            ImplementGetId(domainObjectImplementation);

            //ImplementGetContainedObject(writer);
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

        private void ImplementBeforeCommit(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method(x => x.BeforeCommit).Implement(w => {
                if ( HaveTriggerMethods(_onValidate) || HaveTriggerMethods(_beforeSave) )
                {
                    w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                        WriteTriggerMethodCalls(w, _onValidate);
                        WriteTriggerMethodCalls(w, _beforeSave);
                    });
                }

                if ( HaveTriggerMethods(_beforeDelete) )
                {
                    w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                        WriteTriggerMethodCalls(w, _beforeDelete);
                    });
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementAfterCommit(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method(x => x.AfterCommit).Implement(w => {
                if ( HaveTriggerMethods(_afterSave) )
                {
                    w.If(w.This<IDomainObject>().Prop(x => x.State) != EntityState.RetrievedDeleted).Then(() => {
                        WriteTriggerMethodCalls(w, _afterSave);
                    });
                }

                if ( HaveTriggerMethods(_afterDelete) )
                {
                    w.If(w.This<IDomainObject>().Prop(x => x.State) == EntityState.RetrievedDeleted).Then(() => {
                        WriteTriggerMethodCalls(w, _afterDelete);
                    });
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementEntityId(ImplementationClassWriter<IDomainObject> writer)
        {
            if ( _context.MetaType.BaseType != null )
            {
                //using ( TT.CreateScope<TT.TBase>(writer.OwnerClass.BaseType) )
                //{
                //    var entityIdPropertyInfo = ExpressionUtility.GetPropertyInfo<IDomainObject>(x => x.EntityId);
                //    var baseEntityIdProperty =
                //        TypeMemberCache.Of(writer.OwnerClass.BaseType).Properties.First(p => p.Name == entityIdPropertyInfo.Name && p.DeclaringType == entityIdPropertyInfo.DeclaringType);

                //    writer.Property<object>(intf => intf.EntityId).Implement(p => p.Get(w => {
                //        w.Return(w.This<TT.TBase>().Prop<object>(baseEntityIdProperty));
                //    }));
                //}

                return;
            }

            var idProperty = _context.MetaType.EntityIdProperty;

            if ( idProperty == null )
            {
                writer.Property<object>(intf => intf.EntityId).Implement(p => p.Get(w => {
                    w.Return(w.Const<object>(null));
                }));

                return;
            }

            var idBackingField = writer.OwnerClass.GetPropertyBackingField(idProperty.ContractPropertyInfo);

            writer.Property<object>(intf => intf.EntityId).Implement(p => p.Get(w => {
                using ( TT.CreateScope<TT.TContract, TT.TKey>(_context.MetaType.ContractType, idProperty.ClrType) )
                {
                    w.Return(idBackingField.AsOperand<TT.TKey>().CastTo<object>());
                }
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementGetId(ImplementationClassWriter<IDomainObject> writer)
        {
            if ( _context.MetaType.BaseType != null )
            {
                return;
            }

            var idProperty = _context.MetaType.EntityIdProperty;

            if ( idProperty == null )
            {
                writer.Method<IEntityId>(intf => intf.GetId).Implement(w => {
                    w.Return(w.Const<IEntityId>(null));
                });

                return;
            }

            var idBackingField = writer.OwnerClass.GetPropertyBackingField(idProperty.ContractPropertyInfo);

            writer.Method<IEntityId>(intf => intf.GetId).Implement(w => {
                using ( TT.CreateScope<TT.TContract, TT.TKey>(_context.MetaType.ContractType, idProperty.ClrType) )
                {
                    w.Return(w.New<EntityId<TT.TContract, TT.TKey>>(idBackingField.AsOperand<TT.TKey>()));
                }
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementSetLazyLoader(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method<IPersistableObjectLazyLoader>(x => x.SetLazyLoader).Implement((w, lazyLoader) => {
                _context.ThisLazyLoaderField.Assign(lazyLoader);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementInitializeValues(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method<bool>(x => x.InitializeValues).Implement((w, idManuallyAssigned) => {
                _context.PropertyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteInitialization(w, _componentsField, idManuallyAssigned);
                    });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementImportValues(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method<IEntityRepository, object[]>(x => x.ImportValues).Implement((w, repo, values) => {
                _context.PropertyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteImportStorageValue(w, repo, values);
                    });
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementExportValues(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method<IEntityRepository, object[]>(x => x.ExportValues).Implement((w, repo) => {
                var values = w.Local<object[]>();
                values.Assign(w.NewArray<object>(w.Const(_context.MetaType.Properties.Count)));
                
                _context.PropertyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteExportStorageValue(w, repo, values);
                    });

                w.Return(values);
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

        private void WriteOnNewTriggerMethodCalls(ConstructorWriter writer)
        {
            WriteTriggerMethodCalls(writer, _onNew);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementValidate(ImplementationClassWriter<IDomainObject> writer)
        {
            writer.Method(x => x.Validate).Implement(w => {
                _context.PropertyMap.InvokeStrategies(
                    strategy => {
                        strategy.WriteValidation(w);
                    });

                WriteTriggerMethodCalls(w, _onValidate);
            });
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private void ImplementGetContainedObject(ImplementationClassWriter<TT.TBase> writer)
        //{
        //    writer.ImplementInterfaceExplicitly<IContain<IPersistableObject>>()
        //        .Method<IPersistableObject>(intf => intf.GetContainedObject).Implement(w =>
        //            w.Return(_context.PersistableObjectField.CastTo<IPersistableObject>())
        //        );
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryFindEntityTriggerMethods(Type baseType)
        {
            var members = TypeMemberCache.Of(baseType);

            _onNew = TryFindTriggerMethods<EntityImplementation.TriggerOnNewAttribute>(members, TriggerMethodNameOnNew);
            _onValidate = TryFindTriggerMethods<EntityImplementation.TriggerOnValidateInvariantsAttribute>(members, TriggerMethodNameOnValidateInvariants);
            _beforeSave = TryFindTriggerMethods<EntityImplementation.TriggerBeforeSaveAttribute>(members, TriggerMethodNameBeforeSave);
            _afterSave = TryFindTriggerMethods<EntityImplementation.TriggerAfterSaveAttribute>(members, TriggerMethodNameAfterSave);
            _beforeDelete = TryFindTriggerMethods<EntityImplementation.TriggerBeforeDeleteAttribute>(members, TriggerMethodNameBeforeDelete);
            _afterDelete = TryFindTriggerMethods<EntityImplementation.TriggerAfterDeleteAttribute>(members, TriggerMethodNameAfterDelete);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MethodInfo[] TryFindTriggerMethods<TAttribute>(TypeMemberCache members, string methodName)
            where TAttribute : Attribute
        {
            var foundMethods = members
                .SelectVoids(m => 
                    m.GetParameters().Length == 0 &&
                    !m.IsPublic && 
                    (m.Name == methodName || m.HasAttribute<TAttribute>()))
                .ToArray();

            return foundMethods;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementToString(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var keyProperty = (_context.MetaType.PrimaryKey != null ? _context.MetaType.PrimaryKey.Properties.FirstOrDefault() : null);
            var displayProperty = _context.MetaType.DefaultDisplayProperties.FirstOrDefault();

            if ( keyProperty == null )
            {
                return;
            }

            writer.Method<string>(x => x.ToString).Implement(w => {
                w.Return(
                    w.Const(_context.MetaType.Name + "[") +
                    TypeFactoryUtility.GetPropertyStringValueOperand(w, keyProperty) +
                    (displayProperty != null ? "|" : "") +
                    TypeFactoryUtility.GetPropertyStringValueOperand(w, displayProperty) + "]");
            });
        }
    }
}