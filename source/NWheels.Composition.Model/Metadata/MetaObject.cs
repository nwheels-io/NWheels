using System;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public abstract class MetaObject
    {
        public string Name { get; set; }

        public string QualifiedName { get; set; }
        
        public Type Compiled { get; set; }
        
        public TypeMember Source { get; set; }
        
        public MetaObject Parent { get; set; }
    }
}
