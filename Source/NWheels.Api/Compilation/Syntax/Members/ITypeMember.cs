using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace NWheels.Api.Compilation.Syntax.Members
{
    public interface ITypeMember : IMember
    {
        string AssemblyName { get; }
        string Namespace { get; }
        string FullName { get; }
        ITypeMember BaseClass { get; }
        IReadOnlyList<ITypeMember> Interfaces { get; }
        TypeMemberKind TypeKind { get; }
        Type Binding { get; }
        bool CanMutate { get; }
        bool IsAbstract { get; }
        bool IsValueType { get; }
        bool IsArray { get; }
        bool IsNullable { get; }
        bool IsGenericType { get; }
        bool IsGenericTypeDefinition { get; }
        ITypeMember GenericTypeDefinition { get; }
        IReadOnlyList<ITypeMember> GenericTypeArguments { get; }
        ITypeMember UnderlyingType { get; }
    }
}
