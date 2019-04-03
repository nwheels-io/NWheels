using System;
using System.Collections.Generic;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public class PreprocessedTypeMember
    {
        public PreprocessedTypeMember(
            PreprocessedTypeMember parent,
            MetaAttribute parentAttribute,
            string name,
            string qualifiedName,
            TypeMember abstraction,
            TypeMember concreteMember,
            IReadOnlyList<MetaAttribute> attributes,
            IReadOnlyList<PreprocessedTypeMember> children,
            IReadOnlyDictionary<TypeMember, IReadOnlyList<PreprocessedTypeMember>> childrenByAbstraction)
        {
            Parent = parent;
            ParentAttribute = parentAttribute;
            Name = name;
            QualifiedName = qualifiedName;
            Abstraction = abstraction;
            ConcreteType = concreteMember;
            Attributes = attributes;
            Children = children;
            ChildrenByAbstraction = childrenByAbstraction;
        }

        public PreprocessedTypeMember Parent { get; }

        public MetaAttribute ParentAttribute { get; }

        public string Name { get; }

        public string QualifiedName { get; }
        
        public TypeMember Abstraction { get; }
        
        public TypeMember ConcreteType { get; }
        
        public IReadOnlyList<MetaAttribute> Attributes { get; }

        public IReadOnlyList<PreprocessedTypeMember> Children { get; }
        
        public IReadOnlyDictionary<TypeMember, IReadOnlyList<PreprocessedTypeMember>> ChildrenByAbstraction { get; }
    }
}
