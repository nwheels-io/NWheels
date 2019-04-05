using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms.Expressions;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public class PreprocessedType
    {
        public PreprocessedType()
        {
        }

        public PreprocessedType(TypeMember concreteType, ModelParserInfo parserInfo)
        {
            this.ConcreteType = concreteType;
            this.BaseType = concreteType.BaseType;
            this.Abstraction = parserInfo.Abstraction;
            this.ParserType = parserInfo.ParserType;
        }

        public TypeMember Abstraction { get; set; }
        public TypeMember ConcreteType { get; set; }
        public TypeMember BaseType { get; set; }
        public TypeMember ParserType { get; set; }
        public Type ParserClrType { get; set; }
        public MetadataObject ParsedMetadata { get; set; } 
        public List<PreprocessedTypeArgument> BaseGenericArguments { get; } = new List<PreprocessedTypeArgument>();
        public List<PreprocessedArgument> BaseConstructorArguments { get; } = new List<PreprocessedArgument>();
        public List<PreprocessedPropertyGroup> PropertyGroups { get; } = new List<PreprocessedPropertyGroup>();
    }

    public class PreprocessedPropertyGroup
    {
        public PreprocessedPropertyGroup()
        {
        }
        
        public PreprocessedPropertyGroup(TypeMember discriminator)
        {
            Discriminator = discriminator;
        }

        public TypeMember Discriminator { get; set; }
        public List<PreprocessedProperty> Properties { get; } = new List<PreprocessedProperty>();
    }
    
    public class PreprocessedProperty
    {
        public string Name { get; set; }
        public TypeMember Type { get; set; }
        public AbstractExpression Initializer { get; set; }
        
        public List<PreprocessedTypeArgument> GenericArguments { get; } = new List<PreprocessedTypeArgument>(); 
        public List<PreprocessedArgument> ConstructorArguments { get; } = new List<PreprocessedArgument>();
        public PreprocessedTechnologyAdapter TechnologyAdapter { get; set; }
    }

    public class PreprocessedTechnologyAdapter
    {
        public TypeMember Type { get; set; }
        public Type ClrType { get; set; }
        public AbstractExpression Initializer { get; set; }
        public List<PreprocessedTypeArgument> GenericArguments { get; } = new List<PreprocessedTypeArgument>();
        public List<PreprocessedArgument> AdapterArguments { get; } = new List<PreprocessedArgument>();
    }

    public class PreprocessedTypeArgument
    {
        public string Name { get; set; }
        
        public TypeMember Type { get; set; }

        public static IEnumerable<PreprocessedTypeArgument> FromTypeArguments(TypeMember type) =>
            type.GenericArguments.Select(FromGenericArgumentOf(type));
        
        public static Func<TypeMember, int, PreprocessedTypeArgument> FromGenericArgumentOf(TypeMember type) =>
            (arg, index) => new PreprocessedTypeArgument {
                Name = type.GenericParameters[index].Name,
                Type = arg
            };
    }

    public class PreprocessedArgument
    {
        public string Name { get; set; }
        public bool HasClrValue { get; set; }
        public object ClrValue { get; set; }
        public AbstractExpression ValueExpression { get; set; }

        public static IEnumerable<PreprocessedArgument> FromCallArguments(MethodCallExpression call, int skip = 0) =>
            call.Arguments.Select(FromArgumentOf(call.Method)).Skip(skip);

        public static Func<Argument, int, PreprocessedArgument> FromArgumentOf(MethodMemberBase method) => 
            (arg, index) => new PreprocessedArgument {
                Name = method.Signature.Parameters[index].Name,
                ValueExpression = arg.Expression,
                HasClrValue = (arg.Expression as ConstantExpression)?.Value != null,
                ClrValue = (arg.Expression as ConstantExpression)?.Value
            };
    }
}
