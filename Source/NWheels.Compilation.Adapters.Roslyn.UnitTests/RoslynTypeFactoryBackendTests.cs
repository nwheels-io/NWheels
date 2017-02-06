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
        public void CanReportCompilationIssuesPerTypeMember()
        {
            //-- arrange

            var backendUnderTest = new RoslynTypeFactoryBackend();
            backendUnderTest.EnsureTypeReferenced(this.GetType());

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
            loadedProducts[0].Artifact.For<IFirstContract>().NewInstance().Should().BeOfType<FirstContractImplementation>();

            loadedProducts[1].Key.Should().Be(expectedKey2);
            loadedProducts[1].Artifact.TypeKey.Should().Be(expectedKey2);
            loadedProducts[1].Artifact.RunTimeType.Should().BeSameAs(typeof(SecondContractImplementation));
            loadedProducts[1].Artifact.For<ISecondContract>().NewInstance().Should().BeOfType<SecondContractImplementation>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGenerateHardCodedArtifactClass()
        {
            //-- arrange

            var artifactType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "FirstArtifact");
            artifactType.BaseType = new TypeMember(typeof(RuntimeTypeFactoryArtifact<>).MakeGenericType(typeof(IFirstContract)));

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
                MemberModifier.Override, 
                nameof(IRuntimeTypeFactoryArtifact<object>.NewInstance), 
                new MethodSignature { ReturnValue = new MethodParameter { Type = typeof(IFirstContract) } });

            factoryMethod.Body = new BlockStatement(
                new ReturnStatement { Expression = new NewObjectExpression { Type = typeof(FirstContractImplementation) } }
            );

            artifactType.Members.Add(constructor);
            artifactType.Members.Add(factoryMethod);

            //-- act

            var syntaxEmitter = new ClassSyntaxEmitter(artifactType);
            var actualSyntax = syntaxEmitter.EmitSyntax();

            //-- assert

            var expectedCode = @"
                public class FirstArtifact : 
                    NWheels.Compilation.Mechanism.Factories.RuntimeTypeFactoryArtifact<NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract>
                {
                    public FirstArtifact() : base(
                        new NWheels.Compilation.Mechanism.Factories.TypeKey(
                            typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.TestFactory), 
                            typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract), 
                            null, null, 0, 0, 0), 
                        typeof(NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.FirstContractImplementation)) 
                    { 
                    }
                    public override NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.IFirstContract NewInstance() 
                    { 
                        return new NWheels.Compilation.Adapters.Roslyn.UnitTests.RoslynTypeFactoryBackendTests.FirstContractImplementation();
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

        public interface IFirstContract { }
        public interface ISecondContract { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FirstContractImplementation : IFirstContract {  }
        public class SecondContractImplementation : ISecondContract { }
    }
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

public class ThisAssemblyArtifactCatalog : RuntimeTypeFactoryArtifactCatalog
{
    public override RuntimeTypeFactoryArtifact[] GetArtifacts()
    {
        var artifacts = new RuntimeTypeFactoryArtifact[2];

        artifacts[0] = new FirstArtifact();
        artifacts[1] = new SecondArtifact();

        return artifacts;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class FirstArtifact : RuntimeTypeFactoryArtifact<IFirstContract>
    {
        public FirstArtifact()
            : base(new TypeKey(typeof(TestFactory), typeof(IFirstContract)), typeof(FirstContractImplementation))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IFirstContract NewInstance()
        {
            return new FirstContractImplementation();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SecondArtifact : RuntimeTypeFactoryArtifact<ISecondContract>
    {
        public SecondArtifact()
            : base(new TypeKey(typeof(TestFactory), typeof(ISecondContract)), typeof(SecondContractImplementation))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ISecondContract NewInstance()
        {
            return new SecondContractImplementation();
        }
    }
}
