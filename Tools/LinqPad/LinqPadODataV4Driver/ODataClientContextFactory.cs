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
    public class ODataClientContextFactory : ConventionObjectFactory
    {
        public ODataClientContextFactory(DynamicModule module)
            : base(module, new ODataClientContextConvention(module))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ImplementClientContext(IEdmModel model)
        {
            return base.GetOrBuildType(new EdmModelTypeKey(model)).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ODataClientContextConvention : ImplementationConvention
        {
            private readonly ODataClientEntityFactory _entityFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ODataClientContextConvention(DynamicModule module) 
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _entityFactory = new ODataClientEntityFactory(module);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                var modelTypeKey = (EdmModelTypeKey) context.TypeKey;
                context.ClassFullName = modelTypeKey.Model.GetGeneratedContextClassFullName();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                var modelTypeKey = (EdmModelTypeKey)base.Context.TypeKey;
                var model = modelTypeKey.Model;

                writer.Constructor<Uri>((w, uri) => w.Base<Uri, string>(uri, w.Const(model.GetEntityNamespace())));

                var allEntityTypes = model.SchemaElements.OfType<IEdmEntityType>().ToArray();

                foreach ( var entityEdmType in allEntityTypes )
                {
                    var entityClrType = _entityFactory.ImplementClientEntity(model, entityEdmType);

                    using ( TT.CreateScope<TT.TItem>(entityClrType) )
                    {
                        writer.NewVirtualWritableProperty<DataServiceQuery<TT.TItem>>(entityEdmType.Name).Implement(
                            p => p.Get(w => {
                                w.If(p.BackingField == w.Const<DataServiceQuery<TT.TItem>>(null)).Then(() =>
                                    p.BackingField.Assign(
                                        w.This<DataServiceContext>().Func<string, DataServiceQuery<TT.TItem>>(
                                            x => x.CreateQuery<TT.TItem>, w.Const(entityEdmType.Name)))
                                );
                                w.Return(p.BackingField);
                            }),
                            p => p.Set((w, value) => { }));
                    }
                }
            }
        }
    }
}
