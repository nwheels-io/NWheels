using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using NWheels.Microservices.Api;

namespace NWheels.Microservices.Api
{
    public class AssemblyLocationMap : IAssemblyLocationMap
    {
        private ImmutableArray<string> _directories = ImmutableArray<string>.Empty;
        private ImmutableDictionary<string, string> _filePathByAssemblyName = ImmutableDictionary<string, string>.Empty;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddDirectory(string directory)
        {
            _directories = _directories.Add(directory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddLocations(IReadOnlyDictionary<string, string> filePathByAssemblyName)
        {
            var currentMap = _filePathByAssemblyName;
            var newPairs = filePathByAssemblyName.Where(kvp => !currentMap.ContainsKey(kvp.Key));
            _filePathByAssemblyName = currentMap.AddRange(newPairs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<string> Directories => _directories;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImmutableDictionary<string, string> FilePathByAssemblyName => _filePathByAssemblyName;
    }
}
