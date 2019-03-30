using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Parsers;

namespace NWheels.Build
{
    public class BuildEngine
    {
        private readonly BuildOptions _options;
        private IContainer _services;
        
        public BuildEngine(BuildOptions options)
        {
            _options = options;
        }

        public void Build()
        {
            var projectAssembly = LoadProjectAssembly();
            var programmingModels = DiscoverProgrammingModels(projectAssembly);
            var parserRegistry = RegisterParsers(programmingModels);
            
            _services = BuildServiceContainer(parserRegistry);

            var workspace = LoadProjectWorkspace();
            var modelUnits = DiscoverModelUnits(workspace);
            
        }

        private Assembly LoadProjectAssembly()
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ProgrammingModelEntryPoint> DiscoverProgrammingModels(Assembly projectAssembly)
        {
            throw new NotImplementedException();
        }

        private ParserContributionRegistry RegisterParsers(IEnumerable<ProgrammingModelEntryPoint> programmingModels)
        {
            throw new NotImplementedException();
        }

        private IContainer BuildServiceContainer(ParserContributionRegistry parsers)
        {
            throw new NotImplementedException();
        }

        private Workspace LoadProjectWorkspace()
        {
            throw new NotImplementedException();
        }
        
        private IEnumerable<TypeMember> DiscoverModelUnits(Workspace workspace)
        {
            throw new NotImplementedException();
        }

        private void PreprocessModelUnits()
        {
            
        }

        private void ParseRootUnits()
        {
            
        }
    }
}
