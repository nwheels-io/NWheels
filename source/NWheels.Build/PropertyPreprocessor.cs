using System.Linq;
using MetaPrograms;
using MetaPrograms.CSharp;
using MetaPrograms.CSharp.Reader;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using MetaPrograms.Statements;
using NWheels.Composition.Model.Impl;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.Build
{
    public class PropertyPreprocessor
    {
        private readonly ImperativeCodeModel _code;
        private readonly RoslynCodeModelReader _reader;
        private readonly PreprocessorOutput _output;
        private readonly TypeMember _technologyAdapterAttributeType;

        public PropertyPreprocessor(ImperativeCodeModel code, RoslynCodeModelReader reader, PreprocessorOutput output)
        {
            _code = code;
            _reader = reader;
            _output = output;
            _technologyAdapterAttributeType = code.GetClrTypeMember<TechnologyAdapterAttribute>();
        }

        public void AddProperty(PreprocessedType outType, PropertyMember inProp)
        {
            var getterBody = ExtractGetterBody();
            var outProp = new PreprocessedProperty {
                Name = inProp.Name
            };

            ParseGetterBody();
            AddToType();

            AbstractExpression ExtractGetterBody()
            {
                return 
                    (inProp.Getter.Body.Statements.First() as ReturnStatement)?.Expression
                    ?? throw new BuildErrorException(inProp, $"expected property getter to be an arrow function");
            }
            
            void ParseGetterBody()
            {
                if (getterBody is MethodCallExpression call)
                {
                    ParseTechnologyAdapterInitializer(call, out var modelInitializer);
                    ParseModelInitializer(modelInitializer);
                }
                else
                {
                    ParseModelInitializer(getterBody);
                }
            }

            void ParseTechnologyAdapterInitializer(MethodCallExpression call, out AbstractExpression modelInitializer)
            {
                var adapterType = GetTechnologyAdapterType(call);
                var outAdapter = new PreprocessedTechnologyAdapter {
                    Type = adapterType,
                    Initializer = call
                };
                
                // TODO: populate GenericArguments
                // missing support in MetaPrograms: generic arguments in MethodCallExpression or MethodMember 

                outAdapter.AdapterArguments.AddRange(PreprocessedArgument.FromCallArguments(call, skip: 1));
                outProp.TechnologyAdapter = outAdapter;

                modelInitializer = call.Arguments[0].Expression;
            }

            TypeMember GetTechnologyAdapterType(MethodCallExpression call)
            {
                var adapterAttribute = call.Method?.TryGetAttribute(_technologyAdapterAttributeType);

                if (adapterAttribute == null || call.Method?.Modifier != MemberModifier.Static || call.Arguments.Count == 0)
                {
                    throw new BuildErrorException(
                        inProp, 
                        $"expected a call to technology adapter, but '{call.SafeGetMethodName()}' is not a technology adapter");
                }

                return _code.GetMemberFromTypeof(adapterAttribute.ConstructorArguments.FirstOrDefault());
            }
            
            void ParseModelInitializer(AbstractExpression initializer)
            {
                var newObj = 
                    (initializer as NewObjectExpression)
                    ?? throw new BuildErrorException(inProp, "expected 'new' operator to create an instance of a model class");

                outProp.Type = newObj.Type;
                outProp.Property = inProp;
                outProp.GenericArguments.AddRange(PreprocessedTypeArgument.FromTypeArguments(newObj.Type));
                outProp.ConstructorArguments.AddRange(PreprocessedArgument.FromCallArguments(newObj.ConstructorCall));
            }

            void AddToType()
            {
                //TODO: pick/add group by discriminator
                var targetGroup = outType.PropertyGroups.First();
                targetGroup.Properties.Add(outProp);
            }
        }
    }
}
