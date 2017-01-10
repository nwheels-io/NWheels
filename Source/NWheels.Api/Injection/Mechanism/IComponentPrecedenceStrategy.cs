using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentPrecedenceStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IFallbackComponentPrecedenceStrategy : IComponentPrecedenceStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IPrimaryComponentPrecedenceStrategy : IComponentPrecedenceStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface INormalComponentPrecedenceStrategy : IComponentPrecedenceStrategy
    {
    }
}
