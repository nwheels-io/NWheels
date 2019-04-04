using System;
using System.Collections.Generic;
using MetaPrograms.Members;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class PreprocessorOutput : IReadOnlyPreprocessorOutput
    {
        private readonly List<PreprocessedType> _all;
        private readonly Dictionary<TypeMember, PreprocessedType> _byConcreteType;
        private readonly Dictionary<TypeMember, List<PreprocessedType>> _byBaseType;
        private readonly Dictionary<TypeMember, List<PreprocessedType>> _byAbstraction;
        private readonly Dictionary<Type, PreprocessedType> _byParserType;
        private readonly List<TypeMember> _parserTypes;
        private readonly List<TypeMember> _technologyAdapterTypes;

        public void AddPreprocessedType(PreprocessedType member)
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

        public IEnumerable<PreprocessedType> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedType> GetByParser<TParser>() where TParser : IModelParser
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedType> GetByAbstraction(TypeMember abstraction)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PreprocessedType> GetByBaseType(TypeMember abstraction)
        {
            throw new System.NotImplementedException();
        }

        public PreprocessedType TryGetByConcreteType(TypeMember type)
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