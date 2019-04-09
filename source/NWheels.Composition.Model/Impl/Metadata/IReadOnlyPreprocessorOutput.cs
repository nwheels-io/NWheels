using System.Collections.Generic;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public interface IReadOnlyPreprocessorOutput
    {
        IEnumerable<PreprocessedType> GetAll();
        IEnumerable<PreprocessedType> FindByParser<TParser>() where TParser : IModelParser;
        IEnumerable<PreprocessedType> GetByAbstraction(TypeMember abstraction);
        IEnumerable<PreprocessedType> GetByBaseType(TypeMember abstraction);
        PreprocessedType GetByConcreteType(TypeMember type);
        PreprocessedType TryGetByConcreteType(TypeMember type);
        Workspace Workspace { get; }
        RoslynCodeModelReader CodeReader { get; }
        ImperativeCodeModel Code { get; }
    }
}
