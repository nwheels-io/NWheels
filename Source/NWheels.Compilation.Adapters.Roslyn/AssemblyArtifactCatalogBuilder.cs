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
        private List<TypeMember> _catalogTypes;

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

            ImplementArtifactConstructor(productType, key, artifactType);
            ImplementActivationInterfaces(productType, activationContract, artifactType);

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

        private void ImplementArtifactConstructor(TypeMember productType, TypeKey key, TypeMember artifactType)
        {
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

            artifactType.Members.Add(constructor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementActivationInterfaces(TypeMember productType, Type activationContract, TypeMember artifactType)
        {
            var productConstructors = productType.Members.OfType<ConstructorMember>().ToArray();

            if (productConstructors.Length > 0)
            {
                foreach (var productConstructor in productConstructors)
                {
                    ImplementActivationInterface(productType, activationContract, productConstructor.Signature, artifactType);
                }
            }
            else
            {
                ImplementActivationInterface(productType, activationContract, new MethodSignature(), artifactType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ImplementActivationInterface(
            TypeMember productType,
            TypeMember activationContract,
            MethodSignature constructorSignature,
            TypeMember artifactType)
        {
            var parameterCount = constructorSignature.Parameters.Count;

            var iConstructorGenericParameters =
                constructorSignature.Parameters.Select(p => p.Type)
                .Append(activationContract);

            TypeMember iConstructorInterfaceOpen = _s_iConstructorByParameterCount[parameterCount];
            TypeMember iConstructorInterfaceClosed = iConstructorInterfaceOpen.MakeGenericType(iConstructorGenericParameters.ToArray());

            artifactType.Interfaces.Add(iConstructorInterfaceClosed);

            var factoryMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.None,
                nameof(IConstructor<object>.NewInstance),
                new MethodSignature {
                    ReturnValue = new MethodParameter { Type = activationContract }
                }
            );

            factoryMethod.Signature.Parameters.AddRange(constructorSignature.Parameters);

            var newObjectExpression = new NewObjectExpression {
                Type = productType,
                ConstructorCall = new MethodCallExpression()
            };

            newObjectExpression.ConstructorCall.Arguments.AddRange(
                constructorSignature.Parameters.Select(p => new Argument { Expression = new ParameterExpression { Parameter = p } }));

            factoryMethod.Body = new BlockStatement(
                new ReturnStatement {
                    Expression = newObjectExpression
                }
            );

            var singletonMethod = new MethodMember(
                MemberVisibility.Public,
                MemberModifier.None,
                nameof(IConstructor<object>.GetOrCreateSingleton),
                new MethodSignature { ReturnValue = new MethodParameter { Type = activationContract } });

            singletonMethod.Signature.Parameters.AddRange(constructorSignature.Parameters);

            singletonMethod.Body = new BlockStatement(
                new ThrowStatement { Exception = new NewObjectExpression { Type = typeof(NotSupportedException) } }
            );

            artifactType.Members.Add(factoryMethod);
            artifactType.Members.Add(singletonMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TypeNeedsArtifact(TypeMember type)
        {
            return (type.Status == MemberStatus.Generator && type.Generator.TypeKey.HasValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly ImmutableArray<Type> _s_iConstructorByParameterCount = ImmutableArray<Type>.Empty.AddRange(new[] {
            typeof(IConstructor<>),
            typeof(IConstructor<,>),
            typeof(IConstructor<,,>),
            typeof(IConstructor<,,,>),
            typeof(IConstructor<,,,,>),
            typeof(IConstructor<,,,,,>),
            typeof(IConstructor<,,,,,,>),
            typeof(IConstructor<,,,,,,,>),
            typeof(IConstructor<,,,,,,,,>),
            typeof(IConstructor<,,,,,,,,,>)
        });
    }
}
