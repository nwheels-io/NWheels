using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Writers;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using TT = Hapil.TypeTemplate;

namespace LinqPadODataV4Driver
{
    public class DynamicDataServiceContextFactory : ConventionObjectFactory
    {
        private readonly IEdmModel _model;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicDataServiceContextFactory(DynamicModule module, IEdmModel model)
            : base(module, new DynamicDataContextConvention(module, model))
        {
            _model = model;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type BuildDynamicDataServiceContext()
        {
            return base.GetOrBuildType(new TypeKey()).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DynamicDataContextConvention : ImplementationConvention
        {
            private readonly IEdmModel _model;
            private readonly string _dynamicTypesNamespace;
            private readonly EntityClrTypeCache _clrTypeCache;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DynamicDataContextConvention(DynamicModule module, IEdmModel model) 
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _model = model;
                _dynamicTypesNamespace = model.GetEntityNamespace();
                _clrTypeCache = new EntityClrTypeCache(module, model);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = typeof(DynamicDataServiceContextBase);
                context.ClassFullName = _model.GetGeneratedContextClassFullName();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                writer.Constructor<Uri>((w, uri) => w.Base<Uri, string>(uri, w.Const(_dynamicTypesNamespace)));

                var allEntityTypes = _model.SchemaElements.OfType<IEdmEntityType>().ToArray();

                foreach ( var entityEdmType in allEntityTypes )
                {
                    using ( TT.CreateScope<TT.TItem>(_clrTypeCache.GetEntityClrType(entityEdmType)) )
                    {
                        writer.NewVirtualWritableProperty<DataServiceQuery<TT.TItem>>(entityEdmType.Name).Implement(
                            p => p.Get(w => {
                                w.If(p.BackingField == w.Const<DataServiceQuery<TT.TItem>>(null)).Then(() => {
                                    p.BackingField.Assign(
                                        w.This<DataServiceContext>().Func<string, DataServiceQuery<TT.TItem>>(
                                            x => x.CreateQuery<TT.TItem>, w.Const(entityEdmType.Name)));
                                });
                                w.Return(p.BackingField);
                            }),
                            p => p.Set((w, value) => { }));
                    }
                }
            }
        }
    }
}
