using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.CSharp.Reader.Reflection;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class BuildEngine
    {
        private readonly BuildOptions _options;
        //private IContainer _services;
        
        public BuildEngine(BuildOptions options)
        {
            _options = options;
        }

        public bool Build()
        {
            Console.WriteLine($"Starting build: project {Path.GetFileNameWithoutExtension(_options.ProjectFilePath)}");

            var workspace = LoadProjectWorkspace();
            var codeModel = ReadCodeModel(workspace);
            var declarations = DiscoverModelDeclarations(codeModel);

            declarations.ToList().ForEach(decl => Console.WriteLine($"Discovered type: {decl.FullName}"));

//            var projectAssembly = LoadProjectAssembly();
//            var modelAssemblies = DiscoverProgrammingModelAssemblies(projectAssembly).ToArray();
//            var modelEntryPoints = LoadAllModelEntryPoints(modelAssemblies);
//            var parserRegistry = RegisterParsers(modelEntryPoints);
//            
//            _services = BuildServiceContainer(parserRegistry);
//
//            var declarations = DiscoverModelDeclarations(workspace);
//            var metadata = ParseModelDeclarations(declarations);
//
//            metadata.ToList().ForEach(obj => 
//                Console.WriteLine($"Parsed root meta-object: [{obj.QualifiedName}](${obj.GetType().FullName})"));

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

        private ProgrammingModelEntryPoint[] LoadAllModelEntryPoints(IEnumerable<Assembly> modelAssemblies)
        {
            var allEntryPoints = modelAssemblies
                .SelectMany(LoadProgrammingModelEntryPoints)
                .ToArray();

            Console.WriteLine($"Loaded {allEntryPoints.Length} programming model entry point(s)");
            
            return allEntryPoints;
        }

        private IEnumerable<ProgrammingModelEntryPoint> LoadProgrammingModelEntryPoints(Assembly modelAssembly)
        {
            var attributes = modelAssembly.GetCustomAttributes<ProgrammingModelAttribute>();
            var entryPoints = new List<ProgrammingModelEntryPoint>();
            
            foreach (var attribute in attributes)
            {
                var entryPoint = (ProgrammingModelEntryPoint)Activator.CreateInstance(attribute.EntryPointClass);
                Console.WriteLine($"Loaded model entry point: {entryPoint.GetType().FullName}");

                entryPoints.Add(entryPoint);
            }

            return entryPoints;
        }

        private ModelParserRegistry RegisterParsers(IEnumerable<ProgrammingModelEntryPoint> programmingModels)
        {
            var registry = new ModelParserRegistry();
            
            foreach (var entryPoint in programmingModels)
            {
                Console.WriteLine($"Invoking model entry point: {entryPoint.GetType().FullName}");
                entryPoint.ContributeParsers(registry);
            }

            return registry;
        }

        private IContainer BuildServiceContainer(ModelParserRegistry modelParsers)
        {
            var builder = new ContainerBuilder();
            modelParsers.RegisterParsers(builder);
            return builder.Build();
        }

        private Workspace LoadProjectWorkspace()
        {
            Console.WriteLine("--- project loader: starting ---");
            
            var loader = new BuildalyzerWorkspaceLoader();
            var workspace = loader.LoadWorkspace(new[] { _options.ProjectFilePath });

            Console.WriteLine("--- project loader: success ---");
            
            return workspace;
        }
        
        private ImperativeCodeModel ReadCodeModel(Workspace workspace)
        {
            var reader = new RoslynCodeModelReader(workspace, new ClrTypeResolver());
            reader.Read();

            var codeModel = reader.GetCodeModel();
            return codeModel;
        }

        private IEnumerable<TypeMember> DiscoverModelDeclarations(ImperativeCodeModel codeModel)
        {
            return codeModel.TopLevelMembers.OfType<TypeMember>();
        }

        private IEnumerable<MetaObject> ParseModelDeclarations(IEnumerable<TypeMember> declarations)
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
