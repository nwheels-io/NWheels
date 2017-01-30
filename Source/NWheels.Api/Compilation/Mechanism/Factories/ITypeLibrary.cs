using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeLibrary<TArtifact>
    {
        TypeMember GetOrBuildTypeMember(TypeKey key, Func<TypeKey, TypeMember> factory);

        ITypeFactoryContext CreateFactoryContext<TContextExtension>(TypeKey key, TypeMember type, TContextExtension extension);

        void DeclareTypeMember(TypeKey key, TypeMember type);

        void CompileDeclaredTypeMembers();

        TypeFactoryProduct<TArtifact> GetProduct(ref TypeKey key);
    }
}
