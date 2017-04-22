using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;

namespace NWheels.Compilation
{
    public class CompilationFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.RegisterComponentType<TypeLibrary<IRuntimeTypeFactoryArtifact>>()
                .SingleInstance()
                .ForService<ITypeLibrary<IRuntimeTypeFactoryArtifact>>();

            newComponents.RegisterComponentType<VoidTypeFactoryBackend>()
                .SingleInstance()
                .ForService<ITypeLibrary<IRuntimeTypeFactoryArtifact>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class VoidTypeFactoryBackend : ITypeLibrary<IRuntimeTypeFactoryArtifact>
        {
            public void CompileDeclaredTypeMembers()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ITypeFactoryContext CreateFactoryContext<TContextExtension>(TypeKey key, TypeMember type, TContextExtension extension)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void DeclareTypeMember(TypeKey key, TypeMember type)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeMember GetOrBuildTypeMember(TypeKey key, Func<TypeKey, TypeMember> factory)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeFactoryProduct<IRuntimeTypeFactoryArtifact> GetProduct(ref TypeKey key)
            {
                throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void PopulateProducts(params TypeFactoryProduct<IRuntimeTypeFactoryArtifact>[] products)
            {
                throw new NotImplementedException();
            }
        }
    }
}
