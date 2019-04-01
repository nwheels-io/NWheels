using System.Reflection;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public abstract class MetaProperty
    {
        public string Name { get; set; }

        public string QualifiedName { get; set; }

        public MetaObject Owner { get; }
        
        public PropertyInfo Compiled { get; set; }
        
        public PropertyMember Source { get; set; }
    }
}
