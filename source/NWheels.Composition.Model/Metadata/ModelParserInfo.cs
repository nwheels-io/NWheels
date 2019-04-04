using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public class ModelParserInfo
    {
        public ModelParserInfo(TypeMember abstraction, TypeMember parserType)
        {
            Abstraction = abstraction;
            ParserType = parserType;
        }

        public TypeMember Abstraction { get; }
        public TypeMember ParserType { get; }
    }
}