using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;

namespace NWheels.TypeModel.Core.Factories
{
    public abstract class PropertyImplementationStrategyDecorator : IPropertyImplementationStrategy
    {
        private readonly IPropertyImplementationStrategy _target;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PropertyImplementationStrategyDecorator(IPropertyImplementationStrategy target)
        {
            _target = target;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IPropertyImplementationStrategy

        public virtual void WritePropertyImplementation(ImplementationClassWriter<TypeTemplate.TInterface> implementationWriter)
        {
            _target.WritePropertyImplementation(implementationWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteInitialization(MethodWriterBase initializationConstructorWriter, Operand<IComponentContext> components, params IOperand[] args)
        {
            _target.WriteInitialization(initializationConstructorWriter, components, args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteMaterialization(MethodWriterBase materializationConstructorWriter)
        {
            _target.WriteMaterialization(materializationConstructorWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteResolveDependencies(ClassWriterBase classWriter, MethodWriterBase methodWriter, Operand<IComponentContext> components)
        {
            _target.WriteResolveDependencies(classWriter, methodWriter, components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteSerializingCallback(MethodWriterBase callbackMethodWriter)
        {
            _target.WriteSerializingCallback(callbackMethodWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteDeepListNestedObjects(MethodWriterBase writer, Operand<HashSet<object>> nestedObjects)
        {
            _target.WriteDeepListNestedObjects(writer, nestedObjects);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteDeserializedCallback(MethodWriterBase callbackMethodWriter)
        {
            _target.WriteDeserializedCallback(callbackMethodWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteReturnTrueIfModified(FunctionMethodWriter<bool> functionWriter)
        {
            _target.WriteReturnTrueIfModified(functionWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void WriteValidation(MethodWriterBase writer)
        {
            _target.WriteValidation(writer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ObjectFactoryContext FactoryContext
        {
            get { return _target.FactoryContext; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ITypeMetadataCache MetadataCache
        {
            get { return _target.MetadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ITypeMetadata MetaType
        {
            get { return _target.MetaType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual IPropertyMetadata MetaProperty
        {
            get { return _target.MetaProperty; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool HasDependencies
        {
            get { return _target.HasDependencies; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual bool HasNestedObjects
        {
            get { return _target.HasNestedObjects; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IPropertyImplementationStrategy Target
        {
            get { return _target; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return this.GetType().Name + " { " + _target.ToString() + " }";
        }
    }
}
