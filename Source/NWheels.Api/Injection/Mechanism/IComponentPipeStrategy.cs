using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentPipeStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAfterComponentPipeStrategy : IComponentPipeStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBeforeComponentPipeStrategy : IComponentPipeStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IFirstComponentPipeStrategy : IComponentPipeStrategy
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ILastComponentPipeStrategy : IComponentPipeStrategy
    {
    }
}
