using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NWheels.Compilation.Mechanism.Syntax.Members;
using FluentAssertions;
using System.Linq;
using NWheels.Exceptions;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Factories
{
    public class TypeLibraryTests
    {
        [Fact]
        public void CanCreateFactoryContext()
        {
            //-- arrange

            var libraryUnderTest = new TypeLibrary<TestArtifact>(new TestBackend());

            var key = new TypeKey(
                factoryType: this.GetType(),
                primaryContract: typeof(string));
                
            var type = new TypeMember();
            var extension = new TestContextExtension();

            //-- act

            var factoryContext = libraryUnderTest.CreateFactoryContext<TestContextExtension>(key, type, extension);

            //-- assert

            factoryContext.Should().NotBeNull();
            factoryContext.Key.Should().Be(key);
            factoryContext.Type.Should().BeSameAs(type);
            factoryContext.Extension.Should().BeSameAs(extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanDeclareAndCompileTypeMembers()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            var key3 = new TypeKey(this.GetType(), typeof(ITestContractC));
            var type3 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key3));

            var key4 = new TypeKey(this.GetType(), typeof(ITestContractD));
            var type4 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key4));

            //-- act

            libraryUnderTest.DeclareTypeMember(key1, type1);
            libraryUnderTest.DeclareTypeMember(key2, type2);

            backend.ExpectCompile(type1, type2);
            libraryUnderTest.CompileDeclaredTypeMembers();

            libraryUnderTest.DeclareTypeMember(key3, type3);
            libraryUnderTest.DeclareTypeMember(key4, type4);

            backend.ExpectCompile(type3, type4);
            libraryUnderTest.CompileDeclaredTypeMembers();

            // this should do nothing
            libraryUnderTest.CompileDeclaredTypeMembers();

            var product1 = libraryUnderTest.GetProduct(ref key1);
            var product2 = libraryUnderTest.GetProduct(ref key2);
            var product3 = libraryUnderTest.GetProduct(ref key3);
            var product4 = libraryUnderTest.GetProduct(ref key4);

            //-- assert

            product1.Artifact.SourceType.Should().BeSameAs(type1);
            product2.Artifact.SourceType.Should().BeSameAs(type2);
            product3.Artifact.SourceType.Should().BeSameAs(type3);
            product4.Artifact.SourceType.Should().BeSameAs(type4);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetProductForCompiledTypeMember()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            backend.ExpectCompile(type1, type2);

            libraryUnderTest.DeclareTypeMember(key1, type1);
            libraryUnderTest.DeclareTypeMember(key2, type2);
            libraryUnderTest.CompileDeclaredTypeMembers();

            //-- act

            var product1 = libraryUnderTest.GetProduct(ref key1);
            var product2 = libraryUnderTest.GetProduct(ref key2);

            //-- Assert

            product1.Artifact.SourceType.Should().BeSameAs(type1);
            product2.Artifact.SourceType.Should().BeSameAs(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetProductForLoadedTypeMember()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            backend.RaiseProductsLoaded(
                new TypeFactoryProduct<TestArtifact>(key1, new TestArtifact(type1)),
                new TypeFactoryProduct<TestArtifact>(key2, new TestArtifact(type2))
            );

            //-- act

            var product1 = libraryUnderTest.GetProduct(ref key1);
            var product2 = libraryUnderTest.GetProduct(ref key2);

            //-- Assert

            product1.Artifact.SourceType.Should().BeSameAs(type1);
            product2.Artifact.SourceType.Should().BeSameAs(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanPopulateProducts()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            libraryUnderTest.PopulateProducts(
                new TypeFactoryProduct<TestArtifact>(key1, new TestArtifact(type1)),
                new TypeFactoryProduct<TestArtifact>(key2, new TestArtifact(type2))
            );

            //-- act

            var product1 = libraryUnderTest.GetProduct(ref key1);
            var product2 = libraryUnderTest.GetProduct(ref key2);

            //-- Assert

            product1.Artifact.SourceType.Should().BeSameAs(type1);
            product2.Artifact.SourceType.Should().BeSameAs(type2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanBuildUndeclaredTypeMember()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(string));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var callbackCount = 0;

            //-- act

            var actualResult = libraryUnderTest.GetOrBuildTypeMember(key1, actualKey => {
                callbackCount++;
                actualKey.Should().Be(key1);
                return type1;
            });

            //-- Assert

            callbackCount.Should().Be(1);
            actualResult.Should().BeSameAs(type1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetDeclaredTypeMember()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(string));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            libraryUnderTest.DeclareTypeMember(key1, type1);

            //-- act

            var actualResult = libraryUnderTest.GetOrBuildTypeMember(key1, actualKey => {
                throw new Exception("Callback should not be invoked");
            });

            //-- Assert

            actualResult.Should().BeSameAs(type1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetTypeMemberForExistingProduct()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(string));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            backend.RaiseProductsLoaded(new TypeFactoryProduct<TestArtifact>(key1, new TestArtifact(type1)));

            //-- act

            var actualResult = libraryUnderTest.GetOrBuildTypeMember(key1, actualKey => {
                throw new Exception("Callback should not be invoked");
            });

            //-- Assert

            actualResult.Should().BeSameAs(type1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CompilationErrorsExceptionThrownWhenSomeTypesFailToCompile()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            var key3 = new TypeKey(this.GetType(), typeof(ITestContractC));
            var type3 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key3));

            var key4 = new TypeKey(this.GetType(), typeof(ITestContractD));
            var type4 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key4));

            libraryUnderTest.DeclareTypeMember(key1, type1);
            libraryUnderTest.DeclareTypeMember(key2, type2);
            libraryUnderTest.DeclareTypeMember(key3, type3);
            libraryUnderTest.DeclareTypeMember(key4, type4);

            backend.ExpectCompileWithErrors(
                new[] { type1, type2, type3, type4 }, 
                success: new[] { true, false, true, false });

            //-- act

            var exception = Assert.Throws<CompilationErrorsException>(() => {
                libraryUnderTest.CompileDeclaredTypeMembers();
            });

            //-- Assert

            exception.TypesFailedToCompile.Should().NotBeNull();
            exception.TypesFailedToCompile.Count.Should().Be(2);
            exception.TypesFailedToCompile[0].Type.Should().BeSameAs(type2);
            exception.TypesFailedToCompile[1].Type.Should().BeSameAs(type4);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SucceededProductsExistAfterFailedCompilation()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            var key3 = new TypeKey(this.GetType(), typeof(ITestContractC));
            var type3 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key3));

            var key4 = new TypeKey(this.GetType(), typeof(ITestContractD));
            var type4 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key4));

            libraryUnderTest.DeclareTypeMember(key1, type1);
            libraryUnderTest.DeclareTypeMember(key2, type2);
            libraryUnderTest.DeclareTypeMember(key3, type3);
            libraryUnderTest.DeclareTypeMember(key4, type4);

            backend.ExpectCompileWithErrors(
                new[] { type1, type2, type3, type4 },
                success: new[] { true, false, true, false });

            //-- act

            Assert.Throws<CompilationErrorsException>(() => {
                libraryUnderTest.CompileDeclaredTypeMembers();
            });

            var product1 = libraryUnderTest.GetProduct(ref key1);
            var product3 = libraryUnderTest.GetProduct(ref key3);

            //-- Assert

            product1.Artifact.SourceType.Should().BeSameAs(type1);
            product3.Artifact.SourceType.Should().BeSameAs(type3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TypesFailedToCompileDoNotExistAsProducts()
        {
            //-- arrange

            var backend = new TestBackend();
            var libraryUnderTest = new TypeLibrary<TestArtifact>(backend);

            var key1 = new TypeKey(this.GetType(), typeof(ITestContractA));
            var type1 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key1));

            var key2 = new TypeKey(this.GetType(), typeof(ITestContractB));
            var type2 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key2));

            var key3 = new TypeKey(this.GetType(), typeof(ITestContractC));
            var type3 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key3));

            var key4 = new TypeKey(this.GetType(), typeof(ITestContractD));
            var type4 = new TypeMember(new TypeGeneratorInfo(this.GetType(), key4));

            libraryUnderTest.DeclareTypeMember(key1, type1);
            libraryUnderTest.DeclareTypeMember(key2, type2);
            libraryUnderTest.DeclareTypeMember(key3, type3);
            libraryUnderTest.DeclareTypeMember(key4, type4);

            backend.ExpectCompileWithErrors(
                new[] { type1, type2, type3, type4 },
                success: new[] { true, false, true, false });

            //-- act

            Assert.Throws<CompilationErrorsException>(() => {
                libraryUnderTest.CompileDeclaredTypeMembers();
            });

            //-- Assert

            Assert.Throws<KeyNotFoundException>(() => {
                libraryUnderTest.GetProduct(ref key2);
            });

            Assert.Throws<KeyNotFoundException>(() => {
                libraryUnderTest.GetProduct(ref key4);
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestArtifact
        {
            public TestArtifact(TypeMember sourceType)
            {
                this.SourceType = sourceType;
            }
            public TypeMember SourceType { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestContextExtension
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestBackend : ITypeFactoryBackend<TestArtifact>
        {
            private TypeMember[] _expectedCompile;
            private bool[] _expectedSuccess;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CompilationResult Compile(ICollection<TypeMember> types)
            {
                _expectedCompile.Should().NotBeNull("Unexpected call to backend Compile()");
                _expectedCompile.Should().BeEquivalentTo(types, "Unexpected types in call to backend Compile()");
                _expectedCompile = null;

                var succeeded = new List<TypeCompilationResult>();
                var failed = new List<TypeCompilationResult>();
                var products = new List<TypeFactoryProduct<TestArtifact>>();
                var index = 0;

                foreach (var type in types.Where(t => t.Generator.TypeKey.HasValue))
                {
                    if (_expectedSuccess[index++])
                    {
                        var artifact = new TestArtifact(type);
                        succeeded.Add(new TypeCompilationResult<TestArtifact>(type, true, artifact, new CompilationDiagnostic[0]));
                        products.Add(new TypeFactoryProduct<TestArtifact>(type.Generator.TypeKey.Value, artifact));
                    }
                    else
                    {
                        failed.Add(new TypeCompilationResult<TestArtifact>(type, false, null, new[] {
                            new CompilationDiagnostic(CompilationDiagnosticSeverity.Error, "ERRTEST01", "Test error", "source.cs(1,1)")
                        }));
                    }
                }

                if (products.Count > 0)
                {
                    ProductsLoaded?.Invoke(products.ToArray());
                }

                return new CompilationResult(succeeded, failed, new List<CompilationDiagnostic>());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeMember GetBoundTypeMember(TypeFactoryProduct<TestArtifact> product)
            {
                return product.Artifact.SourceType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExpectCompile(params TypeMember[] types)
            {
                _expectedCompile = types;
                _expectedSuccess = types.Select(t => true).ToArray();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ExpectCompileWithErrors(TypeMember[] types, bool[] success)
            {
                _expectedCompile = types;
                _expectedSuccess = success;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RaiseProductsLoaded(params TypeFactoryProduct<TestArtifact>[] products)
            {
                ProductsLoaded?.Invoke(products);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action<TypeFactoryProduct<TestArtifact>[]> ProductsLoaded;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITestContractA { }
        public interface ITestContractB { }
        public interface ITestContractC { }
        public interface ITestContractD { }
    }
}
