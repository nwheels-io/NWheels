using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetaPrograms;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.CSharp.Reader.Reflection;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class BuildEngine
    {
        private readonly BuildOptions _options;

        public BuildEngine(BuildOptions options)
        {
            _options = options;
        }

        public bool Build()
        {
            Console.WriteLine($"Starting build: project {Path.GetFileNameWithoutExtension(_options.ProjectFilePath)}");

            var workspace = LoadProjectWorkspace();
            var (code, reader) = ReadSourceCode();
            var preprocessor = PreprocessModels();
            var typeLoader = new ClrTypeLoader(reader);
            var parsers = LoadParsers();
            var technologyAdapters = LoadTechnologyAdapters();
            var metadata = ParseModels();

            GenerateOutputs();

            return true;

            Workspace LoadProjectWorkspace()
            {
                Console.WriteLine("--- project loader: starting ---");

                var loader = new BuildalyzerWorkspaceLoader();
                var result = loader.LoadWorkspace(new[] {_options.ProjectFilePath});

                Console.WriteLine("--- project loader: success ---");

                return result;
            }

            (ImperativeCodeModel code, RoslynCodeModelReader reader) ReadSourceCode()
            {
                var resultReader = new RoslynCodeModelReader(workspace, new ClrTypeResolver());

                resultReader.Read();
                var resultCode = resultReader.GetCodeModel();

                return (resultCode, resultReader);
            }

            IReadOnlyPreprocessorOutput PreprocessModels()
            {
                var resultPreprocessor = new Preprocessor(code, reader);
                return resultPreprocessor.Run();
            }

            IDictionary<Type, IModelParser> LoadParsers()
            {
                Console.WriteLine("--- loading parsers ---");
                
                var loaderSession = new ClrTypeLoaderSession(typeLoader);

                foreach (var type in preprocessor.GetAll())
                {
                    var typeRefCopy = type;
                    
                    loaderSession.AddRequest(
                        typeRefCopy.ParserType,
                        clrType => {
                            typeRefCopy.ParserClrType = clrType;
                        });
                }

                var result = loaderSession.LoadRequestedTypes<IModelParser>();

                Console.WriteLine($"Loaded {result.Count} parser(s)");
                return result;
            }

            IDictionary<Type, ITechnologyAdapter> LoadTechnologyAdapters()
            {
                Console.WriteLine("--- loading technology adapters ---");

                var loaderSession = new ClrTypeLoaderSession(typeLoader);

                foreach (var type in preprocessor.GetAll())
                {
                    foreach (var propGroup in type.PropertyGroups)
                    {
                        foreach (var prop in propGroup.Properties.Where(p => p.TechnologyAdapter != null))
                        {
                            var propRefCopy = prop;

                            loaderSession.AddRequest(
                                prop.TechnologyAdapter.Type,
                                clrType => {
                                    propRefCopy.TechnologyAdapter.ClrType = clrType;
                                });
                        }
                    }
                }

                var result = loaderSession.LoadRequestedTypes<ITechnologyAdapter>();

                Console.WriteLine($"Loaded {result.Count} technology adapter(s)");
                return result;
            }

            IMetadataObject[] ParseModels()
            {
                var outputs = new List<MetadataObject>();
                
                Console.WriteLine($"--- parsing: pre-creating metadata ---");
                PreCreateMetadataObjects(outputs);

                Console.WriteLine($"--- parsing: populating metadata ---");
                PopulateMetadataObjects(outputs);
                
                return outputs.Cast<IMetadataObject>().ToArray();
            }

            void PreCreateMetadataObjects(List<MetadataObject> outputs)
            {
                RunModelParsers(outputs, (context, parser) => {
                    Console.Write($"[{context.Input.ConcreteType.FullName}] -> [{parser.GetType().FullName}] ");

                    var metaObject = parser.CreateMetadataObject(context);
                    if (metaObject != null)
                    {
                        context.AllOutputs.Add(metaObject);
                    }
                    
                    context.Input.ParsedMetadata = metaObject;

                    Console.WriteLine($"-> [{metaObject?.GetType().FullName ?? "NULL"}] ");
                });
            }

            void PopulateMetadataObjects(List<MetadataObject> outputs)
            {
                RunModelParsers(outputs, (context, parser) => {
                    Console.WriteLine(
                        $"[{context.Output?.Header.QualifiedName ?? "NULL"} : {context.Output?.GetType().Name ?? "NULL"}] " + 
                        $"-> [{parser.GetType().FullName}]");
                    
                    parser.Parse(context);
                });
            }
            
            void RunModelParsers(
                List<MetadataObject> allOutputs,
                Action<ModelParserContext, IModelParser> action)
            {
                var rootContext = new ModelParserContext(code, reader, preprocessor, allOutputs);

                foreach (var type in preprocessor.GetAll())
                {
                    var parser = parsers[type.ParserClrType];
                    var typeContext = rootContext.WithInput(type);
                    
                    action(typeContext,parser);
                }
            }

            void GenerateOutputs()
            {
                var outputs = new CodeGeneratorOutput(_options);
                
                Console.WriteLine("--- generating codebase ---");

                RunTechnologyAdapters<ITechnologyAdapter>(outputs, (context, adapter) => {
                    adapter.GenerateOutputs(context);
                });

                Console.WriteLine("--- scripting deployments ---");
                
                RunTechnologyAdapters<IDeploymentTechnologyAdapter>(outputs, (context, adapter) => {
                    adapter.GenerateDeploymentOutputs(context);
                });
            }
            
            void RunTechnologyAdapters<TBase>(CodeGeneratorOutput outputs, Action<TechnologyAdapterContext, TBase> action) 
                where TBase : ITechnologyAdapter
            {
                foreach (var metaObject in metadata)
                {
                    foreach (var metaAdapter in metaObject.Header.TechnologyAdapters)
                    {
                        if (!(technologyAdapters[metaAdapter.AdapterType] is TBase adapter))
                        {
                            continue;
                        }

                        var context = new TechnologyAdapterContext(preprocessor, metaObject, outputs);

                        Console.WriteLine(
                            $"[{context.Input?.Header.QualifiedName ?? "NULL"} : {context.Input?.GetType().Name ?? "NULL"}] " + 
                            $"-> [{adapter.GetType().FullName}]");
                        
                        action(context, adapter);
                    }
                }
            }
        }
    }
}

//        
//        private Assembly LoadProjectAssembly()
//        {
//            var assemblyFilePath = Path.Combine(
//                Path.GetDirectoryName(_options.ProjectFilePath),
//                "bin",
//                "Debug",
//                "netstandard2.0",
//                Path.GetFileNameWithoutExtension(_options.ProjectFilePath) + ".dll"
//            );
//            
//            Console.WriteLine($"Loading assembly: {assemblyFilePath}");
//            
//            var assembly = Assembly.LoadFrom(assemblyFilePath);
//            
//            Console.WriteLine($"Loaded assembly: {assembly.FullName}");
//            
//            return assembly;
//        }
//

//        private IEnumerable<TypeMember> DiscoverProgrammindModelTypes()
//        {
//            
//        }
        
//        private IEnumerable<Assembly> DiscoverProgrammingModelAssemblies(Assembly projectAssembly)
//        {
//            var referencedAssemblies = LoadReferencedAssemblies(projectAssembly);
//            var modelAssemblies = new List<Assembly>();
//            
//            foreach (var referencedAssembly in referencedAssemblies)
//            {
//                if (referencedAssembly.IsDefined(typeof(ProgrammingModelAttribute)))
//                {
//                    modelAssemblies.Add(referencedAssembly);
//                    Console.WriteLine($"Loaded programming model assembly: {referencedAssembly.FullName}");
//                }
//            }
//            
//            Console.WriteLine($"Loaded {modelAssemblies.Count} programming model assemblies");
//            return modelAssemblies;
//        }
//
//        private ProgrammingModelEntryPoint[] LoadModelEntryPoints(IEnumerable<Assembly> modelAssemblies)
//        {
//            var allEntryPoints = modelAssemblies
//                .SelectMany(LoadEntryPointsFromAssembly)
//                .ToArray();
//
//            Console.WriteLine($"Loaded {allEntryPoints.Length} programming model entry point(s)");
//            
//            return allEntryPoints;
//        }
//
//        private IEnumerable<ProgrammingModelEntryPoint> LoadEntryPointsFromAssembly(Assembly modelAssembly)
//        {
//            var attributes = modelAssembly.GetCustomAttributes<ProgrammingModelAttribute>();
//            var entryPoints = new List<ProgrammingModelEntryPoint>();
//            
//            foreach (var attribute in attributes)
//            {
//                var entryPoint = (ProgrammingModelEntryPoint)Activator.CreateInstance(attribute.EntryPointClass);
//                Console.WriteLine($"Loaded model entry point: {entryPoint.GetType().FullName}");
//
//                entryPoints.Add(entryPoint);
//            }
//
//            return entryPoints;
//        }
//
//        private ModelParserRegistry RegisterParsers(IEnumerable<ProgrammingModelEntryPoint> programmingModels)
//        {
//            var registry = new ModelParserRegistry();
//            
//            foreach (var entryPoint in programmingModels)
//            {
//                Console.WriteLine($"Invoking model entry point: {entryPoint.GetType().FullName}");
//                entryPoint.ContributeParsers(registry);
//            }
//
//            return registry;
//        }
//
//        private IContainer BuildServiceContainer(ModelParserRegistry modelParsers)
//        {
//            var builder = new ContainerBuilder();
//            modelParsers.RegisterParsers(builder);
//            return builder.Build();
//        }
//
//        private IEnumerable<MetaElement> ParseModelDeclarations(IEnumerable<TypeMember> declarations)
//        {
//            throw new NotImplementedException();
//        }
//
//        private IEnumerable<Assembly> LoadReferencedAssemblies(Assembly assembly)
//        {
//            var allExportedTypes = assembly.GetTypes();
//            var referencedAssemblies = new HashSet<Assembly>();
//            
//            foreach (var type in allExportedTypes)
//            {
//                if (type.BaseType != null && type.BaseType.Assembly != assembly)
//                {
//                    referencedAssemblies.Add(type.BaseType.Assembly);
//
//                    if (type.BaseType.BaseType != null)
//                    {
//                        referencedAssemblies.Add(type.BaseType.BaseType.Assembly);
//                    }
//                }
//            }
//
//            return referencedAssemblies;
//        }


//            modelMembers.Select(decl => {
//                var typeSymbol = decl.Bindings.OfType<INamedTypeSymbol>().FirstOrDefault();
//                if (typeSymbol != null && typeSymbol.ContainingAssembly != null)
//                {
//                    var reference =reader.Compilations
//                        .Select(c => c.GetMetadataReference(typeSymbol.ContainingAssembly))
//                        .OfType<PortableExecutableReference>()
//                        .FirstOrDefault();
//
//                    return (symbol: typeSymbol, path: reference?.FilePath);
//                    //typeSymbol.ContainingAssembly.Identity
//                }
//
//                return (symbol: typeSymbol, path: null);
//            }).ToList().ForEach(item => {
//                var (symbol, path) = item;
//                Console.WriteLine($"{symbol.Name} -> {path ?? "???"}");
//            });

            
            
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

//
//
//        private IEnumerable<TypeMember> DiscoverParseableMembers(ImperativeCodeModel codeModel)
//        {
//            var parseableMembers = codeModel
//                .TopLevelMembers
//                .OfType<TypeMember>()
//                .Where(IsParseableMember)
//                .Select(member => {
//                    Console.WriteLine($"Discovered parseable member: {member.FullName}");
//                    return member;
//                })
//                .ToArray();
//
//            Console.WriteLine($"Discovered {parseableMembers.Length} parseable member(s).");
//            return parseableMembers;
//
//            bool IsParseableMember(TypeMember member)
//            {
//                return (
//                    member.AssemblyName != null && 
//                    !member.AssemblyName.StartsWith("NWheels.") &&
//                    member.BaseType?.Namespace?.StartsWith("NWheels.") == true);
//            }
//        }
//
//        private IEnumerable<PortableExecutableReference> DiscoverModelAssemblyReferences(
//            RoslynCodeModelReader reader, 
//            IEnumerable<TypeMember> parseableMembers)
//        {
//            return parseableMembers
//                .Select(TryGetModelAbstraction)
//                .Select(reader.TryGetAssemblyPEReference)
//                .Where(reference => reference?.FilePath != null)
//                .Distinct()
//                .ToArray();
//
//            TypeMember TryGetModelAbstraction(TypeMember parseable)
//            {
//                while (parseable != null && !parseable.FullName.StartsWith("NWheels."))
//                {
//                    parseable = parseable.BaseType;
//                }
//
//                return parseable;
//            }
//        }
//
//        
//        private IEnumerable<Assembly> LoadModelAssemblies(IEnumerable<PortableExecutableReference> references)
//        {
//            var assemblies = new List<Assembly>();
//            
//            foreach (var reference in references)
//            {
//                var assembly = Assembly.LoadFrom(reference.FilePath);
//                assemblies.Add(assembly);
//
//                Console.WriteLine($"Loaded programming model assembly: {assembly.FullName}");
//            }
//            
//            Console.WriteLine($"Loaded {assemblies.Count} programming model assemblies");
//            return assemblies;
//        }
