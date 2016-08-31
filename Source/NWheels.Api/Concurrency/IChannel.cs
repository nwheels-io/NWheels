using System;

namespace NWheels.Api.Concurrency
{
    public interface IChannel
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IChannel<T> : IChannel
    {
        IProducerChannel<T> Producer { get; }
        IConsumerChannel<T> Consumer { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IProducerChannel<in T>
    {
        void Send(T item);
        void Close();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IConsumerChannel<out T>
    {
        T Receive();
        T TryReceive(TimeSpan timeout, out bool received);
        bool IsClosed { get; }
    }
}
