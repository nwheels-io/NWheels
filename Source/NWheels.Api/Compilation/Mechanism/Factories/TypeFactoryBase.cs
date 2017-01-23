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
            _library.TypeMemberMissing += OnLibraryTypeMemberMissing;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void DefinePipelineAndExtendFactoryContext(
            TypeKey<TKeyExtension> key,
            List<ITypeFactoryConvention> pipeline,
            out TContextExtension contextExtension);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ITypeLibrary<TArtifact> Library => _library;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnLibraryTypeMemberMissing(TypeMemberMissingEventArgs args)
        {
            var key = (TypeKey<TKeyExtension>)args.Key;
            var type = new TypeMember(new TypeGeneratorInfo(this.GetType(), key));

            _library.DeclareTypeMember(key, type);

            var conventionPipeline = new List<ITypeFactoryConvention>();

            DefinePipelineAndExtendFactoryContext(
                key,
                conventionPipeline,
                out TContextExtension contextExtension);

            var factoryContext = _library.CreateFactoryContext<TContextExtension>(key, type, contextExtension);
            ExecuteConventionPipeline(conventionPipeline, factoryContext);

            args.Type = type;
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
