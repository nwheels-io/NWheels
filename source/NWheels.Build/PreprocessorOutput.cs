using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms.Extensions;
using MetaPrograms.Members;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class PreprocessorOutput : IReadOnlyPreprocessorOutput
    {
        private static readonly PreprocessedType[] EmptyTypes = new PreprocessedType[0];
        
        private readonly List<PreprocessedType> _all = new List<PreprocessedType>();
        private readonly Dictionary<TypeMember, PreprocessedType> _byConcreteType = new Dictionary<TypeMember, PreprocessedType>();
        private readonly Dictionary<TypeMember, List<PreprocessedType>> _byAbstraction = new Dictionary<TypeMember, List<PreprocessedType>>();
        private readonly Dictionary<TypeMember, List<PreprocessedType>> _byBaseType = new Dictionary<TypeMember, List<PreprocessedType>>();

        public void AddType(PreprocessedType type)
        {
            _all.Add(type);
            _byConcreteType.Add(type.ConcreteType, type);

            AddToListByKey(type, type.Abstraction, _byAbstraction);

            if (type.BaseType != null)
            {
                AddToListByKey(type, type.BaseType, _byBaseType);
            }
        }

        public IEnumerable<PreprocessedType> GetAll()
        {
            return _all;
        }

        public IEnumerable<PreprocessedType> FindByParser<TParser>() where TParser : IModelParser
        {
            return _all
                .Where(type => type.ParserClrType != null)
                .Where(type => typeof(TParser).IsAssignableFrom(type.ParserClrType));
        }

        public IEnumerable<PreprocessedType> GetByAbstraction(TypeMember abstraction)
        {
            return TryGetListByKey(abstraction, _byAbstraction);
        }

        public IEnumerable<PreprocessedType> GetByBaseType(TypeMember baseType)
        {
            return TryGetListByKey(baseType, _byBaseType);
        }

        public PreprocessedType GetByConcreteType(TypeMember concreteType)
        {
            return _byConcreteType[concreteType];
        }

        public PreprocessedType TryGetByConcreteType(TypeMember concreteType)
        {
            if (_byConcreteType.TryGetValue(concreteType, out var type))
            {
                return type;
            }

            return null;
        }

        private void AddToListByKey<TKey>(PreprocessedType type, TKey key, IDictionary<TKey, List<PreprocessedType>> dictionary)
        {
            List<PreprocessedType> list;

            if (dictionary.TryGetValue(key, out list))
            {
                list.Add(type);
            }
            else
            {
                list = new List<PreprocessedType> { type };
                dictionary.Add(key, list);
            }
        }

        private IEnumerable<PreprocessedType> TryGetListByKey<TKey>(
            TKey key,
            IDictionary<TKey, List<PreprocessedType>> dictionary)
        {
            if (dictionary.TryGetValue(key, out var list))
            {
                return list;
            }
            
            return EmptyTypes;
        }
    }
}