using NWheels.Composition.Model.Metadata;
using NWheels.DevOps.Model.Metadata;

[assembly: ProgrammingModel(typeof(DevOpsModelEntryPoint))]

namespace NWheels.DevOps.Model.Metadata
{
    public class DevOpsModelEntryPoint : ProgrammingModelEntryPoint
    {
        public override void ContributeParsers(IModelParserRegistry modelParsers)
        {
            throw new System.NotImplementedException();
        }
    }
}
