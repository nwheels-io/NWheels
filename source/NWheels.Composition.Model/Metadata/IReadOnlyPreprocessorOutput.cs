using System.Collections.Generic;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public interface IReadOnlyPreprocessorOutput
    {
        IEnumerable<PreprocessedTypeMember> GetAll();
        IEnumerable<PreprocessedTypeMember> GetByParser<TParser>() where TParser : IModelParser;
        IEnumerable<PreprocessedTypeMember> GetByAbstraction(TypeMember abstraction);
        IEnumerable<PreprocessedTypeMember> GetByBaseType(TypeMember abstraction);
        PreprocessedTypeMember TryGetByConcreteType(TypeMember type);
 
        IEnumerable<TypeMember> GetParserTypes();
        IEnumerable<TypeMember> GetTechnologyAdapterTypes();
    }
}
