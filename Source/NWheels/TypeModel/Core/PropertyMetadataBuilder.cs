using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.Exceptions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core
{
    public class PropertyMetadataBuilder : MetadataElement<IPropertyMetadata>, IPropertyMetadata
    {
        private readonly ConcurrentDictionary<Type, PropertyInfo> _implementationPropertyByFactoryType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder()
        {
            _implementationPropertyByFactoryType = new ConcurrentDictionary<Type, PropertyInfo>();

            this.ContractAttributes = new List<PropertyContractAttribute>();
            this.Validation = new PropertyValidationMetadataBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder(TypeMetadataBuilder declaringContract, PropertyInfo declaration, int propertyIndex) 
            : this()
        {
            this.Name = declaration.Name;
            this.ClrType = declaration.PropertyType;
            this.DeclaringContract = declaringContract;
            this.ContractPropertyInfo = declaration;
            this.PropertyIndex = propertyIndex;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMetadataElement Members

        public override string ReferenceName
        {
            get { return this.Name; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IPropertyMetadata Members

        public bool HasContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute
        {
            return this.ContractAttributes.OfType<TAttribute>().Any();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAttribute TryGetContractAttribute<TAttribute>() where TAttribute : PropertyContractAttribute
        {
            return this.ContractAttributes.OfType<TAttribute>().FirstOrDefault();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetImplementation(Type factoryType, out PropertyInfo implementationProperty)
        {
            return _implementationPropertyByFactoryType.TryGetValue(factoryType, out implementationProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<KeyValuePair<Type, PropertyInfo>> GetAllImplementations()
        {
            return _implementationPropertyByFactoryType.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilder DeclaringContract { get; set; }
        public string Name { get; set; }
        public int PropertyIndex { get; private set; }
        public PropertyKind Kind { get; set; }
        public PropertyRole Role { get; set; }
        public Type ClrType { get; set; }
        public ISemanticDataType SemanticType { get; set; }
        public PropertyAccess Access { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsCalculated { get; set; }
        public List<PropertyContractAttribute> ContractAttributes { get; set; }
        public System.Reflection.PropertyInfo ContractPropertyInfo { get; set; }
        public object DefaultValue { get; set; }
        public Type DefaultValueGeneratorType { get; set; }
        public string DefaultDisplayName { get; set; }
        public string DefaultDisplayFormat { get; set; }
        public bool DefaultSortAscending { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsCollection
        {
            get
            {
                return this.ClrType.IsCollectionType();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata IPropertyMetadata.DeclaringContract
        {
            get { return this.DeclaringContract; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<PropertyContractAttribute> IPropertyMetadata.ContractAttributes
        {
            get { return this.ContractAttributes; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IRelationMetadata IPropertyMetadata.Relation
        {
            get { return this.Relation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyValidationMetadata IPropertyMetadata.Validation
        {
            get { return this.Validation; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPropertyRelationalMapping IPropertyMetadata.RelationalMapping
        {
            get { return this.RelationalMapping; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UpdateImplementation(Type factoryType, PropertyInfo implementationProperty)
        {
            _implementationPropertyByFactoryType[factoryType] = implementationProperty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public PropertyRelationalMappingBuilder RelationalMapping { get; set; }
        public RelationMetadataBuilder Relation { get; set; }
        public PropertyValidationMetadataBuilder Validation { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContractQualifiedName
        {
            get
            {
                return (DeclaringContract.Name + "." + this.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(ITypeMetadataVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            PropertyIndex = visitor.VisitAttribute("PropertyIndex", PropertyIndex);
            DeclaringContract = visitor.VisitAttribute("DeclaringContract", DeclaringContract);
            Kind = visitor.VisitAttribute("Kind", Kind);
            Role = visitor.VisitAttribute("Role", Role);
            ClrType = visitor.VisitAttribute("ClrType", ClrType);
            SemanticType = visitor.VisitAttribute("SemanticType", SemanticType);
            Access = visitor.VisitAttribute("Access", Access);
            ContractAttributes = visitor.VisitAttribute("ContractAttributes", ContractAttributes);
            ContractPropertyInfo = visitor.VisitAttribute("ContractPropertyInfo", ContractPropertyInfo);
            DefaultValueGeneratorType = visitor.VisitAttribute("DefaultValueGeneratorType", DefaultValueGeneratorType);
            DefaultDisplayName = visitor.VisitAttribute("DefaultDisplayName", DefaultDisplayName);
            DefaultDisplayFormat = visitor.VisitAttribute("DefaultDisplayFormat", DefaultDisplayFormat);
            DefaultSortAscending = visitor.VisitAttribute("DefaultSortAscending", DefaultSortAscending);

            Relation = visitor.VisitElement<IRelationMetadata, RelationMetadataBuilder>(Relation);
            Validation = visitor.VisitElement<IPropertyValidationMetadata, PropertyValidationMetadataBuilder>(Validation);
            RelationalMapping = visitor.VisitElement<IPropertyRelationalMapping, PropertyRelationalMappingBuilder>(RelationalMapping);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetDefaultValueOperand(MethodWriterBase writer, out IOperand<TT.TProperty> valueOperand)
        {
            if ( DefaultValue != null )
            {
                if ( ClrType.IsInstanceOfType(DefaultValue) )
                {
                    if ( DefaultValue is System.Type )
                    {
                        valueOperand = 
                            Static.Func<string, bool, Type>(Type.GetType, writer.Const(((Type)DefaultValue).AssemblyQualifiedName), writer.Const(true))
                            .CastTo<TT.TProperty>();
                    }
                    else
                    {
                        var valueOperandType = typeof(Constant<>).MakeGenericType(DefaultValue.GetType());
                        valueOperand = ((IOperand)Activator.CreateInstance(valueOperandType, DefaultValue)).CastTo<TT.TProperty>();
                    }
                }
                else if ( DefaultValue is string )
                {
                    valueOperand = Static.Func(ParseUtility.Parse<TT.TProperty>, writer.Const((string)DefaultValue));
                }
                else
                {
                    throw new ContractConventionException(
                        this.GetType(),
                        ContractPropertyInfo.DeclaringType,
                        ContractPropertyInfo,
                        "Specified default value could not be parsed");
                }

                return true;
            }

            valueOperand = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return Name + " : " + ClrType.FriendlyName();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RelationMetadataBuilder SafeGetRelation()
        {
            if ( this.Relation == null )
            {
                this.Relation = new RelationMetadataBuilder();
            }

            return this.Relation;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyRelationalMappingBuilder SafeGetRelationalMapping()
        {
            if ( this.RelationalMapping == null )
            {
                this.RelationalMapping = new PropertyRelationalMappingBuilder();
            }

            return this.RelationalMapping;
        }
    }
}
