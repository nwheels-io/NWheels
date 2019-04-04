using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Extensions;
using MetaPrograms.Members;

namespace NWheels.Build
{
    public class ClrTypeLoader
    {
        private readonly RoslynCodeModelReader _reader;
        private readonly Dictionary<string, Assembly> _cachedAssemblies = new Dictionary<string, Assembly>();
        private readonly Dictionary<TypeMember, Type> _cachedTypes = new Dictionary<TypeMember, Type>();
        private readonly Dictionary<TypeMember, Action<Type>> _onTypeLoaded = new Dictionary<TypeMember, Action<Type>>();

        public ClrTypeLoader(RoslynCodeModelReader reader)
        {
            _reader = reader;
        }

        public IDictionary<TypeMember, Type> LoadClrTypes(IEnumerable<TypeMember> types)
        {
            var (result, cacheMisses) = TryFindInCache(types);
            
            var typesByAssemblyRefs = cacheMisses
                .Select(type => (type, reference: _reader.TryGetAssemblyPEReference(type)))
                .Where(tuple => tuple.reference?.FilePath != null)
                .GroupBy(tuple => tuple.reference, tuple => tuple.type)
                .ToArray();
                
            foreach (var typesInAssembly in typesByAssemblyRefs)
            {
                var assemblyRef = typesInAssembly.Key;
                var assembly = Assembly.LoadFrom(assemblyRef.FilePath);

                foreach (var type in typesInAssembly)
                {
                    var clrType = assembly.GetType(type.FullName, throwOnError: true);
                    _cachedTypes.Add(type, clrType);
                    result.Add(type, clrType);
                }
            }

            return result;
        }

        private (Dictionary<TypeMember, Type> result, IEnumerable<TypeMember> cacheMisses) TryFindInCache(
            IEnumerable<TypeMember> types)
        {
            var matchTuples = types
                .Select(type => (type, clrType: _cachedTypes.GetValueOrDefault(type)))
                .ToArray();

            var cacheHits = matchTuples.Where(tuple => tuple.clrType != null);
            var cacheMisses = matchTuples.Where(tuple => tuple.clrType == null);

            return (
                result: cacheHits.ToDictionary(hit => hit.type, hit => hit.clrType),
                cacheMisses: cacheMisses.Select(miss => miss.type));
        }
    }
}