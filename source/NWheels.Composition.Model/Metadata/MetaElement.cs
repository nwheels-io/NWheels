using System;
using System.Collections.Generic;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public class MetaElement
    {
        public MetaElement(
            MetaElement parent,
            MetaAttribute parentAttribute,
            string name,
            string qualifiedName,
            TypeMember abstraction,
            TypeMember concreteMember,
            IReadOnlyList<MetaAttribute> attributes,
            IReadOnlyList<MetaElement> children,
            IReadOnlyDictionary<TypeMember, IReadOnlyList<MetaElement>> childrenByAbstraction)
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

        public MetaElement Parent { get; }

        public MetaAttribute ParentAttribute { get; }

        public string Name { get; }

        public string QualifiedName { get; }
        
        public TypeMember Abstraction { get; }
        
        public TypeMember ConcreteType { get; }
        
        public IReadOnlyList<MetaAttribute> Attributes { get; }

        public IReadOnlyList<MetaElement> Children { get; }
        
        public IReadOnlyDictionary<TypeMember, IReadOnlyList<MetaElement>> ChildrenByAbstraction { get; }
    }
}
