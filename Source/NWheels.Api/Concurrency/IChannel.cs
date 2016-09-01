using System;

namespace NWheels.Api.Concurrency
{
    public interface IChannel
    {
        string Name 
        { 
            [return: Guard.NotNull]
            get; 
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IProducer<in T>
    {
        void Send(T item);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void Close();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IChannel Channel 
        { 
            [return: Guard.NotNull] 
            get; 
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IConsumer<out T>
    {
        T Receive();
        T TryReceive(TimeSpan timeout, out bool received);
        IChannel Channel { get; }
        bool IsClosed { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IChannel<T> : IChannel
    {
        IProducer<T> Producer { get; }
        IConsumer<T> Consumer { get; }
    }
}
