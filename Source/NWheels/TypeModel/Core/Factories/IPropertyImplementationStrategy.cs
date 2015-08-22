using System.Collections.Generic;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;

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
        ObjectFactoryContext FactoryContext { get; }
        ITypeMetadataCache MetadataCache { get; }
        ITypeMetadata MetaType { get; }
        IPropertyMetadata MetaProperty { get; }
        bool HasDependencies { get; }
        bool HasNestedObjects { get; }
    }
}