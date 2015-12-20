using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Entities;

namespace NWheels.DataObjects.Core.Factories
{
    public interface IPropertyImplementationStrategy
    {
        void WritePropertyImplementation(ImplementationClassWriter<TypeTemplate.TInterface> implementationWriter);
        void WriteInitialization(MethodWriterBase initializationConstructorWriter, Operand<IComponentContext> components, params IOperand[] args);
        void WriteMaterialization(MethodWriterBase materializationConstructorWriter);
        void WriteResolveDependencies(ClassWriterBase classWriter, MethodWriterBase methodWriter, Operand<IComponentContext> components);
        void WriteDeepListNestedObjects(MethodWriterBase writer, Operand<HashSet<object>> nestedObjects);
        void WriteSerializingCallback(MethodWriterBase callbackMethodWriter);
        void WriteDeserializedCallback(MethodWriterBase callbackMethodWriter);
        void WriteReturnTrueIfModified(FunctionMethodWriter<bool> functionWriter);
        void WriteValidation(MethodWriterBase writer);
        void WriteExportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector);
        void WriteImportStorageValue(MethodWriterBase methodWriter, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector);
        ObjectFactoryContext FactoryContext { get; }
        ITypeMetadataCache MetadataCache { get; }
        ITypeMetadata MetaType { get; }
        IPropertyMetadata MetaProperty { get; }
        PropertyInfo ImplementedContractProperty { get; }
        PropertyInfo ImplementedStorageProperty { get; }
        bool HasDependencies { get; }
        bool HasNestedObjects { get; }
    }
}