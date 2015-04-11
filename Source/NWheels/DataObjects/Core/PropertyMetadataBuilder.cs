using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Exceptions;
using NWheels.Utilities;
using TT = Hapil.TypeTemplate;

namespace NWheels.DataObjects.Core
{
    public class PropertyMetadataBuilder : MetadataElement<IPropertyMetadata>, IPropertyMetadata
    {
        public PropertyMetadataBuilder()
        {
            this.ContractAttributes = new List<PropertyContractAttribute>();
            this.Validation = new PropertyValidationMetadataBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMetadataBuilder(PropertyInfo declaration) 
            : this()
        {
            this.Name = declaration.Name;
            this.ClrType = declaration.PropertyType;
            this.ContractPropertyInfo = declaration;
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

        public string Name { get; set; }
        public PropertyKind Kind { get; set; }
        public bool IsKey { get; set; }
        public Type ClrType { get; set; }
        public ISemanticDataType SemanticType { get; set; }
        public PropertyAccess Access { get; set; }
        public bool IsSensitive { get; set; }
        public List<PropertyContractAttribute> ContractAttributes { get; set; }
        public System.Reflection.PropertyInfo ContractPropertyInfo { get; set; }
        public System.Reflection.PropertyInfo ImplementationPropertyInfo { get; set; }
        public object DefaultValue { get; set; }
        public string DefaultDisplayName { get; set; }
        public string DefaultDisplayFormat { get; set; }
        public bool DefaultSortAscending { get; set; }

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
        
        public PropertyRelationalMappingBuilder RelationalMapping { get; set; }
        public RelationMetadataBuilder Relation { get; set; }
        public PropertyValidationMetadataBuilder Validation { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            Name = visitor.VisitAttribute("Name", Name);
            Kind = visitor.VisitAttribute("Kind", Kind);
            IsKey = visitor.VisitAttribute("IsKey", IsKey);
            ClrType = visitor.VisitAttribute("ClrType", ClrType);
            SemanticType = visitor.VisitAttribute("SemanticType", SemanticType);
            Access = visitor.VisitAttribute("Access", Access);
            ContractAttributes = visitor.VisitAttribute("ContractAttributes", ContractAttributes);
            ContractPropertyInfo = visitor.VisitAttribute("ContractPropertyInfo", ContractPropertyInfo);
            ImplementationPropertyInfo = visitor.VisitAttribute("ImplementationPropertyInfo", ImplementationPropertyInfo);
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
    }
}
