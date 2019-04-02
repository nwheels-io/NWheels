using System.Reflection;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public class MetaAttribute
    {
        public MetaAttribute(
            MetaElement owner, 
            string name, 
            string qualifiedName, 
            PropertyMember abstraction, 
            PropertyMember concreteMember,
            MetaElement child)
        {
            Owner = owner;
            Name = name;
            QualifiedName = qualifiedName;
            Abstraction = abstraction;
            ConcreteMember = concreteMember;
            Child = child;
        }

        public MetaElement Owner { get; }

        public string Name { get; }

        public string QualifiedName { get; }
        
        public PropertyMember Abstraction { get; }
        
        public PropertyMember ConcreteMember { get; }
        
        public MetaElement Child { get; } 
    }
}
