using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Policy
{
    public abstract class TypeFactoryBase<TKeyExtension, TContextExtension>
    {
        private readonly ITypeFactoryMechanism _mechanism;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TypeFactoryBase(ITypeFactoryMechanism mechanism)
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

        protected ITypeFactoryMechanism Mechanism => _mechanism;

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
}
