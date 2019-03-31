using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using MetaPrograms.CSharp.Reader;
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

        public bool Build()
        {
            Console.WriteLine($"Starting build: project {Path.GetFileNameWithoutExtension(_options.ProjectFilePath)}");
            
            var projectAssembly = LoadProjectAssembly();
            var programmingModelAssemblies = DiscoverProgrammingModelAssemblies(projectAssembly);
            var programmingModels = programmingModelAssemblies.SelectMany(DiscoverProgrammingModels);
            var parserRegistry = RegisterParsers(programmingModels);
            
            _services = BuildServiceContainer(parserRegistry);

            var workspace = LoadProjectWorkspace();
            var allUnitDeclarations = DiscoverModelUnitDeclarations(workspace);
            var allUnits = PreprocessModelUnits(allUnitDeclarations);
            var parsedRootUnits = ParseRootModelUnits(allUnits);

            parsedRootUnits.ToList().ForEach(unit => 
                Console.WriteLine($"Parsed unit: {unit.FullName} : ${unit.GetType().FullName}"));

            return true;
        }

        private Assembly LoadProjectAssembly()
        {
            var assemblyFilePath = Path.Combine(
                Path.GetDirectoryName(_options.ProjectFilePath),
                "bin",
                "Debug",
                "netstandard2.0",
                Path.GetFileNameWithoutExtension(_options.ProjectFilePath) + ".dll"
            );
            
            Console.WriteLine($"Loading assembly: {assemblyFilePath}");
            
            var assembly = Assembly.LoadFrom(assemblyFilePath);
            
            Console.WriteLine($"Loaded assembly: {assembly.FullName}");
            
            return assembly;
        }

        private IEnumerable<Assembly> DiscoverProgrammingModelAssemblies(Assembly projectAssembly)
        {
            var referencedAssemblies = LoadReferencedAssemblies(projectAssembly);
            var modelAssemblies = new List<Assembly>();
            
            foreach (var referencedAssembly in referencedAssemblies)
            {
                if (referencedAssembly.IsDefined(typeof(ProgrammingModelAttribute)))
                {
                    modelAssemblies.Add(referencedAssembly);
                    Console.WriteLine($"Loaded programming model assembly: {referencedAssembly.FullName}");
                }
            }
            
            Console.WriteLine($"Loaded {modelAssemblies.Count} programming model assemblies");
            return modelAssemblies;
        }

        private IEnumerable<ProgrammingModelEntryPoint> DiscoverProgrammingModels(Assembly programmingModelAssembly)
        {
            var attributes = programmingModelAssembly.GetCustomAttributes<ProgrammingModelAttribute>();
            var entryPoints = new List<ProgrammingModelEntryPoint>();
            
            foreach (var attribute in attributes)
            {
                var entryPoint = (ProgrammingModelEntryPoint)Activator.CreateInstance(attribute.EntryPointClass);
                Console.WriteLine($"Loaded programming model entry point: {entryPoint.GetType().FullName}");

                entryPoints.Add(entryPoint);
            }
            
            Console.WriteLine($"Loaded {entryPoints.Count} programming model entry point(s)");
            return entryPoints;
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
        
        private IEnumerable<TypeMember> DiscoverModelUnitDeclarations(Workspace workspace)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ProgrammingModelUnit> PreprocessModelUnits(IEnumerable<TypeMember> declarations)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ProgrammingModelUnit> ParseRootModelUnits(IEnumerable<ProgrammingModelUnit> modelUnits)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Assembly> LoadReferencedAssemblies(Assembly assembly)
        {
            var allExportedTypes = assembly.GetTypes();
            var referencedAssemblies = new HashSet<Assembly>();
            
            foreach (var type in allExportedTypes)
            {
                if (type.BaseType != null && type.BaseType.Assembly != assembly)
                {
                    referencedAssemblies.Add(type.BaseType.Assembly);

                    if (type.BaseType.BaseType != null)
                    {
                        referencedAssemblies.Add(type.BaseType.BaseType.Assembly);
                    }
                }
            }

            return referencedAssemblies;
        }
    }
}
