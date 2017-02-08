using NWheels.Compilation.Adapters.Roslyn.SyntaxEmitters;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Expressions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using NWheels.Compilation.Mechanism.Syntax.Statements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn
{
    public class AssemblyArtifactCatalogBuilder
    {
        private readonly ImmutableList<TypeMember> _typesToCompile;
        private  List<TypeMember> _catalogTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AssemblyArtifactCatalogBuilder(IEnumerable<TypeMember> typesToCompile)
        {
            _typesToCompile = ImmutableList<TypeMember>.Empty.AddRange(typesToCompile);
            _catalogTypes = new List<TypeMember>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildArtifactCatalog()
        {
            _catalogTypes.Clear();

            var artifactTypes = new List<TypeMember>();

            foreach (var type in _typesToCompile.Where(TypeNeedsArtifact))
            {
                var artifactType = BuildArtifactTypeFor(type);
                artifactTypes.Add(artifactType);
            }

            var catalogType = BuildCatalogTypeFor(artifactTypes);

            _catalogTypes.AddRange(artifactTypes);
            _catalogTypes.Add(catalogType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<TypeMember> CatalogTypes => _catalogTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMember BuildArtifactTypeFor(TypeMember productType)
        {
            var key = productType.Generator.TypeKey.Value;
            var activationContract = productType.Generator.ActivationContract ?? typeof(object);

            var artifactType = new TypeMember(
                MemberVisibility.Public, 
                TypeMemberKind.Class, 
                SyntaxHelpers.GetValidCSharpIdentifier($"FactoryOf_{productType.FullName}"));

            artifactType.BaseType = new TypeMember(typeof(RuntimeTypeFactoryArtifact<>).MakeGenericType(activationContract));

            var constructor = new ConstructorMember(MemberVisibility.Public, MemberModifier.None, artifactType.Name, new MethodSignature());
            constructor.CallBaseConstructor = new MethodCallExpression();

            var typeKeyConstructorCall = new MethodCallExpression();
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.FactoryType } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.PrimaryContract } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.SecondaryContract1 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.SecondaryContract2 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.SecondaryContract3 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.ExtensionValue1 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.ExtensionValue2 } });
            typeKeyConstructorCall.Arguments.Add(new Argument { Expression = new ConstantExpression { Value = key.ExtensionValue3 } });

            constructor.CallBaseConstructor.Arguments.Add(new Argument {
                Expression = new NewObjectExpression {
                    Type = typeof(TypeKey),
                    ConstructorCall = typeKeyConstructorCall
                }
            });
            
            constructor.CallBaseConstructor.Arguments.Add(new Argument {
                Expression = new ConstantExpression { Value = productType }
            });

            var factoryMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.Override,
                nameof(IRuntimeTypeFactoryArtifact<object>.NewInstance),
                new MethodSignature { ReturnValue = new MethodParameter { Type = activationContract } });

            factoryMethod.Body = new BlockStatement(
                new ReturnStatement {
                    //TODO: pass arguments to constructor, if any
                    Expression = new NewObjectExpression { Type = productType }
                }
            );

            artifactType.Members.Add(constructor);
            artifactType.Members.Add(factoryMethod);

            return artifactType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMember BuildCatalogTypeFor(IReadOnlyList<TypeMember> artifactTypes)
        {
            var catalogType = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, RuntimeTypeFactoryArtifactCatalog.ConcreteCatalogClassName);
            catalogType.BaseType = typeof(RuntimeTypeFactoryArtifactCatalog);

            var getArtifactsMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.Override,
                nameof(RuntimeTypeFactoryArtifactCatalog.GetArtifacts),
                new MethodSignature { ReturnValue = new MethodParameter { Type = typeof(RuntimeTypeFactoryArtifact[]) } });

            var artifactsVariable = new LocalVariable { Name = "artifacts" };

            getArtifactsMethod.Body = new BlockStatement();
            getArtifactsMethod.Body.Statements.Add(new VariableDeclarationStatement {
                Variable = artifactsVariable,
                InitialValue = new NewArrayExpression {
                    ElementType = typeof(RuntimeTypeFactoryArtifact),
                    Length = new ConstantExpression { Value = artifactTypes.Count }
                },
            });

            var artifactIndex = 0;

            foreach (var type in artifactTypes)
            {
                getArtifactsMethod.Body.Statements.Add(new ExpressionStatement {
                    Expression = new AssignmentExpression {
                        Left = new IndexerExpression {
                            Target = new LocalVariableExpression { Variable = artifactsVariable },
                            Index = new ConstantExpression { Value = artifactIndex }
                        },
                        Right = new NewObjectExpression { Type = type }
                    }
                });

                artifactIndex++;
            }

            getArtifactsMethod.Body.Statements.Add(new ReturnStatement {
                Expression = new LocalVariableExpression { Variable = artifactsVariable }
            });

            catalogType.Members.Add(getArtifactsMethod);

            return catalogType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TypeNeedsArtifact(TypeMember type)
        {
            return (type.Status == MemberStatus.Generator && type.Generator.TypeKey.HasValue);
        }
    }
}
