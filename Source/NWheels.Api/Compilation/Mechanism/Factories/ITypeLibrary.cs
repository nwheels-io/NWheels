using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeLibrary<TArtifact>
    {
        TypeKey<TKeyExtension> CreateKey<TKeyExtension>(
            TypeMember primaryContract, 
            TypeMember[] secondaryContracts = null, 
            TKeyExtension extension = default(TKeyExtension))
            where TKeyExtension : ITypeKeyExtension, new();

        ITypeFactoryContext CreateFactoryContext<TContextExtension>(TypeKey key, TypeMember type, TContextExtension extension);

        TypeMember GetOrBuildTypeMember(TypeKey key, Func<TypeKey, TypeMember> factory);

        void DeclareTypeMember(TypeKey key, TypeMember type);

        void CompileDeclaredTypeMembers();

        TypeFactoryProduct<TArtifact> GetProduct(TypeKey key);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeMemberMissingEventArgs : EventArgs
    {
        public TypeMemberMissingEventArgs(TypeKey key)
        {
            this.Key = key;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey Key { get; }
        public TypeMember Type { get; set; }
    }
}
