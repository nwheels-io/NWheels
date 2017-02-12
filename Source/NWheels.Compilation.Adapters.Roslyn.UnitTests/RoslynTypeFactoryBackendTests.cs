using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests;
using NWheels.Compilation.Adapters.Roslyn.UnitTests;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Syntax.Statements;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class RoslynTypeFactoryBackendTests
    {
        [Fact]
        public void CanCompileAssembly()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());
            backendUnderTest.EnsureTypeReferenced(typeof(RuntimeTypeFactoryArtifactCatalog));

            var key1 = new TypeKey(this.GetType(), typeof(int), typeof(int), typeof(int), typeof(int), 1, 2, 3);
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            type1.Namespace = "NS1";
            type1.Visibility = MemberVisibility.Public;
            type1.TypeKind = TypeMemberKind.Class;
            type1.Name = "ClassOne";

            //-- act

            var result = backendUnderTest.Compile(new[] { type1 });

            //-- assert

            result.Success.Should().BeTrue();
            result.Succeeded.Count.Should().Be(1);
            result.Succeeded[0].Type.Should().BeSameAs(type1);
            result.Failed.Count.Should().Be(0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInstantiateCompiledProducts()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());
            backendUnderTest.EnsureTypeReferenced(typeof(RuntimeTypeFactoryArtifactCatalog));

            var compiledArtifacts = new Dictionary<TypeKey, IRuntimeTypeFactoryArtifact<object>>();

            backendUnderTest.ProductsLoaded += (products) => {
                foreach (var product in products)
                {
                    compiledArtifacts.Add(product.Key, (IRuntimeTypeFactoryArtifact<object>)product.Artifact);
                }
            };

            var key1 = new TypeKey(this.GetType(), typeof(int));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassInt");

            var key2 = new TypeKey(this.GetType(), typeof(string));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2), "NS2", MemberVisibility.Public, TypeMemberKind.Class, "ClassString");

            //-- act

            var result = backendUnderTest.Compile(new[] { type1, type2 });
            var classInt = compiledArtifacts[key1].Constructor().NewInstance();
            var classString = compiledArtifacts[key2].Constructor().NewInstance();

            //-- assert

            result.Success.Should().BeTrue();
            compiledArtifacts.Count.Should().Be(2);
            classInt.GetType().FullName.Should().Be("NS1.ClassInt");
            classString.GetType().FullName.Should().Be("NS2.ClassString");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInstantiateCompiledProductWithNonDefaultConstructor()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());
            backendUnderTest.EnsureTypeReferenced(typeof(RuntimeTypeFactoryArtifactCatalog));

            var compiledArtifacts = new Dictionary<TypeKey, IRuntimeTypeFactoryArtifact<object>>();

            backendUnderTest.ProductsLoaded += (products) => {
                foreach (var product in products)
                {
                    compiledArtifacts.Add(product.Key, (IRuntimeTypeFactoryArtifact<object>)product.Artifact);
                }
            };

            var key1 = new TypeKey(this.GetType(), typeof(int));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1), "NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var intValueProperty = new PropertyMember { Visibility = MemberVisibility.Public, PropertyType = typeof(int), Name = "IntValue" };
            var stringValueProperty = new PropertyMember { Visibility = MemberVisibility.Public, PropertyType = typeof(string), Name = "StringValue" };
            var constructor = new ConstructorMember(
                MemberVisibility.Public, 
                MemberModifier.None, 
                "ClassOne",
                new MethodSignature(new MethodParameter[] {
                    new MethodParameter("intValue", 1, typeof(int)),
                    new MethodParameter("stringValue", 2, typeof(string)),
                }, null, false)
            );
            constructor.Body = new BlockStatement(
                new ExpressionStatement {
                    Expression = new AssignmentExpression {
                        Left = new MemberExpression { Target = new ThisExpression(), Member = intValueProperty } ,
                        Right = new ParameterExpression { Parameter = constructor.Signature.Parameters.First(p => p.Name == "intValue") },
                    }
                },
                new ExpressionStatement {
                    Expression = new AssignmentExpression {
                        Left = new MemberExpression { Target = new ThisExpression(), Member = stringValueProperty },
                        Right = new ParameterExpression { Parameter = constructor.Signature.Parameters.First(p => p.Name == "stringValue") },
                    }
                }
            );
            type1.Members.Add(constructor);
            type1.Members.Add(intValueProperty);
            type1.Members.Add(stringValueProperty);

            //-- act

            var result = backendUnderTest.Compile(new[] { type1 });
            var obj = compiledArtifacts[key1].Constructor<int, string>().NewInstance(123, "ABC");

            //-- assert

            result.Success.Should().BeTrue();
            compiledArtifacts.Count.Should().Be(1);
            obj.GetType().FullName.Should().Be("NS1.ClassOne");

            dynamic dynamicObj = obj;

            int initializedIntValue = dynamicObj.IntValue;
            string initializedStringValue = dynamicObj.StringValue;

            initializedIntValue.Should().Be(123);
            initializedStringValue.Should().Be("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanReportCompilationIssuesPerTypeMember()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());
            backendUnderTest.EnsureTypeReferenced(typeof(RuntimeTypeFactoryArtifactCatalog));

            var type1 = new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne");
            var type2 = new TypeMember("NS1", MemberVisibility.Private, TypeMemberKind.Class, "ClassTwo");
            var type3 = new TypeMember("NS1", MemberVisibility.Public, TypeMemberKind.Class, "ClassThree");
            var type4 = new TypeMember("NS1", MemberVisibility.Private, TypeMemberKind.Class, "ClassFour");

            //-- act

            var result = backendUnderTest.Compile(new[] { type1, type2, type3, type4 });

            //-- assert

            result.Success.Should().BeFalse();
            result.Succeeded.Count.Should().Be(0);

            result.Failed.Count.Should().Be(4);

            result.Failed[0].Success.Should().BeTrue();
            result.Failed[0].Diagnostics.Count.Should().Be(0);

            result.Failed[1].Success.Should().BeFalse();
            result.Failed[1].Diagnostics.Count.Should().Be(1);
            result.Failed[1].Diagnostics[0].Severity.Should().Be(CompilationDiagnosticSeverity.Error);
            result.Failed[1].Diagnostics[0].Code.Should().Be("CS1527");
            result.Failed[1].Diagnostics[0].Message.Should().NotBeNullOrEmpty();
            result.Failed[1].Diagnostics[0].SourceLocation.Should().NotBeNullOrEmpty();

            result.Failed[2].Success.Should().BeTrue();
            result.Failed[2].Diagnostics.Count.Should().Be(0);

            result.Failed[3].Success.Should().BeFalse();
            result.Failed[3].Diagnostics.Count.Should().Be(1);
            result.Failed[3].Diagnostics[0].Severity.Should().Be(CompilationDiagnosticSeverity.Error);
            result.Failed[3].Diagnostics[0].Code.Should().Be("CS1527");
            result.Failed[3].Diagnostics[0].Message.Should().NotBeNullOrEmpty();
            result.Failed[3].Diagnostics[0].SourceLocation.Should().NotBeNullOrEmpty();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanLoadPrecompiledAssembly()
        {
            //-- arrange

            TypeFactoryProduct<IRuntimeTypeFactoryArtifact>[] loadedProducts = null;
            int productsLoadedInvocationCount = 0;

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.ProductsLoaded += (products) => {
                loadedProducts = products;
                productsLoadedInvocationCount++;
            };

            //-- act

            backendUnderTest.LoadPrecompiledAssembly(this.GetType().GetTypeInfo().Assembly.Location);

            //-- assert

            var expectedKey1 = new TypeKey(typeof(TestFactory), typeof(IFirstContract));
            var expectedKey2 = new TypeKey(typeof(TestFactory), typeof(ISecondContract));

            productsLoadedInvocationCount.Should().Be(1);
            loadedProducts.Should().NotBeNull();
            loadedProducts.Length.Should().Be(2);

            loadedProducts[0].Key.Should().Be(expectedKey1);
            loadedProducts[0].Artifact.TypeKey.Should().Be(expectedKey1);
            loadedProducts[0].Artifact.RunTimeType.Should().BeSameAs(typeof(FirstContractImplementation));

            var instance1 = loadedProducts[0].Artifact.For<IFirstContract>().Constructor().NewInstance();
            instance1.Should().BeOfType<FirstContractImplementation>();

            loadedProducts[1].Key.Should().Be(expectedKey2);
            loadedProducts[1].Artifact.TypeKey.Should().Be(expectedKey2);
            loadedProducts[1].Artifact.RunTimeType.Should().BeSameAs(typeof(SecondContractImplementation));

            var instance2 = loadedProducts[1].Artifact.For<ISecondContract>().Constructor<int, string>().NewInstance(123, "ABC");
            instance2.Should().BeOfType<SecondContractImplementation>();
            instance2.IntValue.Should().Be(123);
            instance2.StringValue.Should().Be("ABC");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGenerateHardCodedArtifactClass()
        {
            //-- arrange

            var artifactType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "FirstArtifact");
            artifactType.BaseType = new TypeMember(typeof(RuntimeTypeFactoryArtifact<>).MakeGenericType(typeof(IFirstContract)));
            artifactType.Interfaces.Add(new TypeMember(typeof(IConstructor<>).MakeGenericType(typeof(IFirstContract))));

            var constructor = new ConstructorMember(MemberVisibility.Public, MemberModifier.None, "FirstArtifact", new MethodSignature());
            constructor.CallBaseConstructor = new MethodCallExpression();

            var typeKeyConstructorCall = new MethodCallExpression();
            typeKeyConstructorCall.Arguments.Add(new Argument {
                Expression = new ConstantExpression { Value = typeof(TestFactory) }
            });
            typeKeyConstructorCall.Arguments.Add(new Argument {
                Expression = new ConstantExpression { Value = typeof(IFirstContract) }
            });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression() });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression() });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = 0 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = 0 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = 0 } });

            constructor.CallBaseConstructor.Arguments.Add(new Argument {
                Expression = new NewObjectExpression {
                    Type = typeof(TypeKey),
                    ConstructorCall = typeKeyConstructorCall
                }
            });
            constructor.CallBaseConstructor.Arguments.Add(new Argument {
                Expression = new ConstantExpression { Value = typeof(FirstContractImplementation) }
            });

            var factoryMethod = new MethodMember(
                MemberVisibility.Public, 
                MemberModifier.None, 
                nameof(IConstructor<object>.NewInstance), 
                new MethodSignature { ReturnValue = new MethodParameter { Type = typeof(IFirstContract) } });

            factoryMethod.Body = new BlockStatement(
                new ReturnStatement { Expression = new NewObjectExpression { Type = typeof(FirstContractImplementation) } }
            );

            var singletonMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.None,
                nameof(IConstructor<object>.GetOrCreateSingleton),
                new MethodSignature { ReturnValue = new MethodParameter { Type = typeof(IFirstContract) } });

            singletonMethod.Body = new BlockStatement(
                new ThrowStatement { Exception = new NewObjectExpression { Type = typeof(NotSupportedException) } }
            );

            artifactType.Members.Add(constructor);
            artifactType.Members.Add(factoryMethod);
            artifactType.Members.Add(singletonMethod);

            //-- act

            var syntaxEmitter = new ClassSyntaxEmitter(artifactType);
            var actualSyntax = syntaxEmitter.EmitSyntax();

            //-- assert

            var expectedCode = @"
                public class FirstArtifact : 
                    NWheels.Compilation.Mechanism.Factories.RuntimeTypeFactoryArtifact<NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract>,
                    NWheels.Compilation.Mechanism.Factories.IConstructor<NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract>
                {
                    public FirstArtifact() : base(
                        new NWheels.Compilation.Mechanism.Factories.TypeKey(
                            typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.TestFactory), 
                            typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract), 
                            null, null, 0, 0, 0), 
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.FirstContractImplementation)) 
                    { 
                    }
                    public NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract NewInstance() 
                    { 
                        return new NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.FirstContractImplementation();
                    }
                    public NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract GetOrCreateSingleton() 
                    { 
                        throw new System.NotSupportedException();
                    }
                }
            ";

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGenerateHardCodedArtifactCatalogClass()
        {
            //-- arrange

            var firstArtifactType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "FirstArtifact");
            var secondArtifactType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "SecondArtifact");

            var catalogType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ThisAssemblyArtifactCatalog");
            catalogType.BaseType = typeof(RuntimeTypeFactoryArtifactCatalog);

            var getArtifactsMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.Override,
                nameof(RuntimeTypeFactoryArtifactCatalog.GetArtifacts),
                new MethodSignature { ReturnValue = new MethodParameter { Type = typeof(RuntimeTypeFactoryArtifact[]) } });

            var artifactsVariable = new LocalVariable { Name = "artifacts" };

            getArtifactsMethod.Body = new BlockStatement(
                new VariableDeclarationStatement {
                    Variable = artifactsVariable,
                    InitialValue = new NewArrayExpression {
                        ElementType = typeof(RuntimeTypeFactoryArtifact),
                        Length = new ConstantExpression { Value = 2 }
                    },
                },
                new ExpressionStatement {
                    Expression = new AssignmentExpression {
                        Left = new IndexerExpression {
                            Target = new LocalVariableExpression { Variable = artifactsVariable },
                            Index = new ConstantExpression { Value = 0 }
                        },
                        Right = new NewObjectExpression { Type = firstArtifactType }
                    }
                },
                new ExpressionStatement {
                    Expression = new AssignmentExpression {
                        Left = new IndexerExpression {
                            Target = new LocalVariableExpression { Variable = artifactsVariable },
                            Index = new ConstantExpression { Value = 1 }
                        },
                        Right = new NewObjectExpression { Type = secondArtifactType }
                    }
                },
                new ReturnStatement {
                    Expression = new LocalVariableExpression { Variable = artifactsVariable } 
                }
            );

            catalogType.Members.Add(getArtifactsMethod);

            //-- act

            var syntaxEmitter = new ClassSyntaxEmitter(catalogType);
            var actualSyntax = syntaxEmitter.EmitSyntax();

            //-- assert

            var expectedCode = @"
                public class ThisAssemblyArtifactCatalog : NWheels.Compilation.Mechanism.Factories.RuntimeTypeFactoryArtifactCatalog
                {
                    public override NWheels.Compilation.Mechanism.Factories.RuntimeTypeFactoryArtifact[] GetArtifacts()
                    {
                        var artifacts = new NWheels.Compilation.Mechanism.Factories.RuntimeTypeFactoryArtifact[2];

                        artifacts[0] = new FirstArtifact();
                        artifacts[1] = new SecondArtifact();

                        return artifacts;
                    }
                }
            ";

            actualSyntax.Should().BeEquivalentToCode(expectedCode);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

#if false
        //TODO later
        [Fact]
        public void CanPersistMethodInfoInTypeKey()
        {
            //-- arrange

            var method = this.GetType().GetMethods().First();

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());

            var sourceKey = new TypeKey(this.GetType(), typeof(string), method.MetadataToken);
            var sourceType = new TypeMember(
                new TypeGeneratorInfo(this.GetType(), sourceKey), 
                "NS1", 
                MemberVisibility.Public, 
                TypeMemberKind.Class, 
                "ClassOne");

            TypeKey loadedKey = new TypeKey();

            backendUnderTest.ProductsLoaded += (products) => {
                var artifact = products[0].Artifact;
                var attribute = artifact.RunTimeType.GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();
                loadedKey = attribute.ToTypeKey();
            };

            backendUnderTest.Compile(new[] { sourceType });

            //TypeBuilder builder

            ////-- act

            //var methodToken = loadedKey.ExtensionValue1;
            //MethodInfo loadedMethod = MethodInfo.GetMethodFromHandle(new RuntimeMethodHandle().)

            ////-- assert

            //result.Success.Should().BeTrue();
            //result.Succeeded.Count.Should().Be(1);
            //result.Succeeded[0].Type.Should().BeSameAs(sourceType);
            //result.Failed.Count.Should().Be(0);
        }
#endif

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestFactory { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IFirstContract
        {
        }
        public interface ISecondContract
        {
            int IntValue { get; }
            string StringValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FirstContractImplementation : IFirstContract
        {
        }
        public class SecondContractImplementation : ISecondContract
        {
            public SecondContractImplementation(int intValue, string stringValue)
            {
                this.IntValue = intValue;
                this.StringValue = stringValue;
            }
            public int IntValue { get; }
            public string StringValue { get; }
        }
    }
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ThisAssemblyFactoryCatalog : RuntimeTypeFactoryArtifactCatalog
{
    public override RuntimeTypeFactoryArtifact[] GetArtifacts()
    {
        var artifacts = new RuntimeTypeFactoryArtifact[2];

        artifacts[0] = new FirstArtifact();
        artifacts[1] = new SecondArtifact();

        return artifacts;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class FirstArtifact : RuntimeTypeFactoryArtifact<IFirstContract>, IConstructor<IFirstContract>
    {
        private object _singletonInstanceSyncRoot = new object();
        private IFirstContract _singletonInstance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FirstArtifact()
            : base(new TypeKey(typeof(TestFactory), typeof(IFirstContract)), typeof(FirstContractImplementation))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFirstContract NewInstance()
        {
            return new FirstContractImplementation();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IFirstContract GetOrCreateSingleton()
        {
            if (_singletonInstance == null)
            {
                lock (_singletonInstanceSyncRoot)
                {
                    if (_singletonInstance == null)
                    {
                        _singletonInstance = new FirstContractImplementation();
                    }
                }
            }

            return _singletonInstance;

        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SecondArtifact : RuntimeTypeFactoryArtifact<ISecondContract>, IConstructor<int, string, ISecondContract>
    {
        private object _singletonInstanceSyncRoot = new object();
        private ISecondContract _singletonInstance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SecondArtifact()
            : base(new TypeKey(typeof(TestFactory), typeof(ISecondContract)), typeof(SecondContractImplementation))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISecondContract NewInstance(int arg1, string arg2)
        {
            return new SecondContractImplementation(arg1, arg2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISecondContract GetOrCreateSingleton(int arg1, string arg2)
        {
            if (_singletonInstance == null)
            {
                lock (_singletonInstanceSyncRoot)
                {
                    if (_singletonInstance == null)
                    {
                        _singletonInstance = new SecondContractImplementation(arg1, arg2);
                    }
                }
            }

            return _singletonInstance;
        }
    }
}
