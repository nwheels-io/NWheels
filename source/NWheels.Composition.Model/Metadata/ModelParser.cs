using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public interface IRootModelParser
    {
    }
    
    public abstract class ModelParser
    {
        public abstract IEnumerable<TypeMember> RegisterToAbstractions(ImperativeCodeModel codeModel);
        public abstract bool  
    }
}
