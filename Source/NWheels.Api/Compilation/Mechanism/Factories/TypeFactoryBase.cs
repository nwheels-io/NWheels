using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class TypeFactoryBase<TKeyExtension, TContextExtension, TArtifact>
        where TKeyExtension : ITypeKeyExtension, new()
    {
        private readonly ITypeLibrary<TArtifact> _library;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeFactoryBase(ITypeLibrary<TArtifact> library)
        {
            _library = library;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey LoadTypeKey(TypeKeyAttribute productAttribute)
        {
            var extension = productAttribute.DeserializeTypeKeyExtension<TKeyExtension>();

            TypeMember[] secondaryContracts;

            if (productAttribute.SecondaryContracts != null)
            {
                secondaryContracts = new TypeMember[productAttribute.SecondaryContracts.Count];

                for (int i = 0 ; i < secondaryContracts.Length ; i++)
                {
                    secondaryContracts[i] = productAttribute.SecondaryContracts[i];
                }
            }
            else
            {
                secondaryContracts = null;
            }

            return _library.CreateKey<TKeyExtension>(
                this.GetType(),
                productAttribute.PrimaryContract,
                secondaryContracts,
                extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DefinePipelineAndExtendFactoryContext(
            TypeKey<TKeyExtension> key,
            List<ITypeFactoryConvention> pipeline,
            out TContextExtension contextExtension);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeMember GetOrBuildTypeMember(TypeKey key)
        {
            return _library.GetOrBuildTypeMember(key, BuildNewTypeMember);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ITypeLibrary<TArtifact> Library => _library;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMember BuildNewTypeMember(TypeKey key)
        {
            var typedKey = (TypeKey<TKeyExtension>)key;
            var type = new TypeMember(new TypeGeneratorInfo(this.GetType(), key));

            _library.DeclareTypeMember(key, type);

            var conventionPipeline = new List<ITypeFactoryConvention>();

            DefinePipelineAndExtendFactoryContext(
                typedKey,
                conventionPipeline,
                out TContextExtension contextExtension);

            var factoryContext = _library.CreateFactoryContext<TContextExtension>(key, type, contextExtension);
            ExecuteConventionPipeline(conventionPipeline, factoryContext);

            return type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteConventionPipeline(List<ITypeFactoryConvention> pipeline, ITypeFactoryContext factoryContext)
        {
            var effectivePipeline = pipeline.Where(sink => sink.ShouldApply(factoryContext)).ToImmutableList();

            foreach (var sink in effectivePipeline)
            {
                sink.Validate(factoryContext);
            }

            foreach (var sink in effectivePipeline)
            {
                sink.Declare(factoryContext);
            }

            foreach (var sink in effectivePipeline)
            {
                sink.Implement(factoryContext);
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TypeFactoryBase<TContextExtension ,TArtifact> : TypeFactoryBase<Empty.KeyExtension, TContextExtension, TArtifact>
    {
        protected TypeFactoryBase(ITypeLibrary<TArtifact> mechanism)
            : base(mechanism)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TypeFactoryBase<TArtifact> : TypeFactoryBase<Empty.KeyExtension, Empty.ContextExtension, TArtifact>
    {
        protected TypeFactoryBase(ITypeLibrary<TArtifact> mechanism)
            : base(mechanism)
        {
        }
    }
}
