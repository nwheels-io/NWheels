using System;

namespace NWheels.Api.Concurrency
{
    public interface IEndSelect
    {
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISelectCase : IEndSelect
    {
        ISelectCase Case<T>(IConsumerChannel<T> channel, Action<T> body);
        IEndSelect Default(Action body);
    }
}
