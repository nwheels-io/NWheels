using System;
using System.Collections.Generic;
using MetaPrograms.Members;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class PreprocessorOutput : IReadOnlyPreprocessorOutput
    {
        private readonly List<PreprocessedTypeMember> _all;
        private readonly Dictionary<TypeMember, PreprocessedTypeMember> _byConcreteType;
        private readonly Dictionary<TypeMember, List<PreprocessedTypeMember>> _byBaseType;
        private readonly Dictionary<TypeMember, List<PreprocessedTypeMember>> _byAbstraction;
        private readonly Dictionary<Type, PreprocessedTypeMember> _byParserType;
        private readonly List<TypeMember> _parserTypes;
        private readonly List<TypeMember> _technologyAdapterTypes;

        public void AddPreprocessedType(PreprocessedTypeMember member)
        {
            throw new System.NotImplementedException();
        }

        public void AddParser(TypeMember type)
        {
            throw new System.NotImplementedException();
        }

        public void AddTechnologyAdapter(TypeMember type)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedTypeMember> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedTypeMember> GetByParser<TParser>() where TParser : IModelParser
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedTypeMember> GetByAbstraction(TypeMember abstraction)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedTypeMember> GetByBaseType(TypeMember abstraction)
        {
            throw new System.NotImplementedException();
        }

        public PreprocessedTypeMember TryGetByConcreteType(TypeMember type)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TypeMember> GetParserTypes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TypeMember> GetTechnologyAdapterTypes()
        {
            throw new NotImplementedException();
        }
    }
}