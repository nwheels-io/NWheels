using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentInstancingStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISingletonComponentInstancingStrategy : IComponentInstancingStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ITransientComponentInstancingStrategy : IComponentInstancingStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPerExecutionPathComponentInstancingStrategy : IComponentInstancingStrategy
    {
    }
}
