using MetaPrograms.Members;

namespace NWheels.Composition.Model.Parsers
{
    public abstract class ProgrammingModelUnit
    {
        protected ProgrammingModelUnit(TypeMember declaration)
        {
            Declaration = declaration;
        }

        public abstract string FullName { get; }
        public TypeMember Declaration { get; }
    }
}
