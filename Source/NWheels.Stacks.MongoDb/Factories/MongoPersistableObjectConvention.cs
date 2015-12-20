using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities;
using NWheels.Entities.Core;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoPersistableObjectConvention : ImplementationConvention
    {
        private readonly PropertyImplementationStrategyMap _propertyMap;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoPersistableObjectConvention(PropertyImplementationStrategyMap propertyMap)
            : base(Will.ImplementBaseClass)
        {
            _propertyMap = propertyMap;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ImplementationConvention

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            writer.DefaultConstructor();

            writer.ImplementInterfaceExplicitly<IObject>()
                .Property<bool>(x => x.IsModified).Implement(
                    p => p.Get(w => 
                        w.Return(w.Const(false))
                    )
                );

            writer.ImplementInterfaceExplicitly<IPersistableObject>()
                .Method<IEntityRepository, object[]>(x => x.ExportValues).Implement(
                    (w, repo) => {
                        var values = w.Local<object[]>();
                        values.Assign(w.NewArray<object>(w.Const(_propertyMap.MetaType.Properties.Count)));

                        _propertyMap.InvokeStrategies(
                            strategy => {
                                strategy.WriteExportStorageValue(w, repo, values);
                            });

                        w.Return(values);
                    })
                .Method<IEntityRepository, object[]>(x => x.ImportValues).Implement(
                    (w, repo, values) => {
                        _propertyMap.InvokeStrategies(
                            strategy => {
                                strategy.WriteImportStorageValue(w, repo, values);
                            });
                    })
                .Property(x => x.EntityId).Implement(p => p.Get(w => {
                    var idProperty = _propertyMap.MetaType.EntityIdProperty;
                    
                    if ( idProperty != null )
                    {
                        using ( TT.CreateScope<TT.TKey>(idProperty.ClrType) )
                        {
                            var idBackingField = w.OwnerClass.GetMemberByName<PropertyMember>(idProperty.Name).BackingField.AsOperand<TT.TKey>();
                            w.Return(idBackingField.CastTo<object>());
                        }
                    }
                    else
                    {
                        w.Return(w.Const<object>(null));
                    }
                }));
        }

        #endregion
    }
}
