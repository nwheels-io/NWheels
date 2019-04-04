using System;
using System.Collections.Generic;
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
        public List<PreprocessedTypeArgument> BaseGenericArguments { get; } 
        public List<PreprocessedArgument> BaseConstructorArguments { get; } 
        public List<PreprocessedPropertyGroup> PropertyGroups { get; } 
    }

    public class PreprocessedPropertyGroup
    {
        public TypeMember Discriminator { get; set; }
        public List<PreprocessedProperty> Properties { get; }
    }
    
    public class PreprocessedProperty
    {
        public string Name { get; set; }
        public TypeMember Type { get; set; }
        public AbstractExpression Initializer { get; set; }
        
        public List<PreprocessedTypeArgument> GenericArguments { get; set; } 
        public List<PreprocessedArgument> ConstructorArguments { get; set; }
        public PreprocessedTechnologyAdapter TechnologyAdapter { get; set; }
    }

    public class PreprocessedTechnologyAdapter
    {
        public TypeMember Type { get; set; }
        public Type ClrType { get; set; }
        public List<PreprocessedTypeArgument> GenericArguments { get; } 
        public List<PreprocessedArgument> ConstructorArguments { get; }
    }

    public class PreprocessedTypeArgument
    {
        public string Name { get; set; }
        public TypeMember Type { get; set; }
    }

    public class PreprocessedArgument
    {
        public string Name { get; set; }
        public bool HasClrValue { get; set; }
        public object ClrValue { get; set; }
        public AbstractExpression ValueExpression { get; set; }
    }
}
