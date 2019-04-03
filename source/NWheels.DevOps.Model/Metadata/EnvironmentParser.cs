using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.Members;
using NWheels.Composition.Model.Metadata;

namespace NWheels.DevOps.Model.Metadata
{
    public class EnvironmentParser: IModelParser, IRootModelParser
    {
        public override IEnumerable<TypeMember> Abstractions(ImperativeCodeModel codeModel)
        {
            throw new System.NotImplementedException();
        }
    }
}
