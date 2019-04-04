using System.Collections.Generic;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public interface IReadOnlyPreprocessorOutput
    {
        IEnumerable<PreprocessedType> GetAll();
        IEnumerable<PreprocessedType> FindByParser<TParser>() where TParser : IModelParser;
        IEnumerable<PreprocessedType> GetByAbstraction(TypeMember abstraction);
        IEnumerable<PreprocessedType> GetByBaseType(TypeMember abstraction);
        PreprocessedType TryGetByConcreteType(TypeMember type);
    }
}
