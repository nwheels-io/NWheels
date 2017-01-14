using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy.Relaxed
{
    public abstract class TypeFactoryBase<TKeyExtension, TContextExtension, TArtifact>
    {
        private readonly ITypeFactoryMechanism<TArtifact> _mechanism;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeFactoryBase(ITypeFactoryMechanism<TArtifact> mechanism)
        {
            _mechanism = mechanism;
            _mechanism.BuildingNewProduct += OnMechanismBuildingNewProduct;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnBuildingNewProduct(
            ITypeKey<TKeyExtension> key,
            List<ITypeFactoryConvention> pipe,
            out TContextExtension contextExtension);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ITypeFactoryMechanism<TArtifact> Mechanism => _mechanism;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnMechanismBuildingNewProduct(BuildingNewTypeEventArgs args)
        {
            OnBuildingNewProduct(
                (ITypeKey<TKeyExtension>)args.Key,
                args.Pipe,
                out TContextExtension contextExtension);

            args.Context = _mechanism.CreateContext(args.Key, contextExtension);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TypeFactoryBase<TContextExtension ,TArtifact> : TypeFactoryBase<Empty.KeyExtension, TContextExtension, TArtifact>
    {
        protected TypeFactoryBase(ITypeFactoryMechanism<TArtifact> mechanism)
            : base(mechanism)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TypeFactoryBase<TArtifact> : TypeFactoryBase<Empty.KeyExtension, Empty.ContextExtension, TArtifact>
    {
        protected TypeFactoryBase(ITypeFactoryMechanism<TArtifact> mechanism)
            : base(mechanism)
        {
        }
    }
}
