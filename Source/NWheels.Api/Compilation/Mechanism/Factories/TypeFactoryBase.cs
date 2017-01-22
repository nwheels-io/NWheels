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
            _library.BuildingNewProduct += OnLibraryBuildingNewProduct;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnBuildingNewProduct(
            ITypeKey<TKeyExtension> key,
            List<ITypeFactoryConvention> pipe,
            out TContextExtension contextExtension);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ITypeLibrary<TArtifact> Library => _library;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnLibraryBuildingNewProduct(BuildingNewProductEventArgs args)
        {
            OnBuildingNewProduct(
                (ITypeKey<TKeyExtension>)args.Key,
                args.Pipe,
                out TContextExtension contextExtension);

            var product = new TypeMember(new TypeGeneratorInfo(this.GetType(), args.Key));
            args.Context = _library.CreateContext<TContextExtension>(args.Key, product, contextExtension);

            ExecuteConventionPipeline(args);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteConventionPipeline(BuildingNewProductEventArgs args)
        {
            var context = args.Context;
            var effectivePipe = args.Pipe.Where(sink => sink.ShouldApply(context)).ToImmutableList();

            foreach (var sink in effectivePipe)
            {
                sink.Validate(args.Context);
            }

            foreach (var sink in effectivePipe)
            {
                sink.Declare(args.Context);
            }

            foreach (var sink in effectivePipe)
            {
                sink.Implement(args.Context);
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
