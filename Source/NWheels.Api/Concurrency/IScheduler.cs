using System;
using NWheels.Concurrency.Advanced;

namespace NWheels.Api.Concurrency
{
    public interface IScheduler
    {
        [return: Guard.NotNull] IChannel<T> NewChannel<T>(
            [Guard.NotEmpty] string name);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        T Receive<T>([Guard.NotNull] IConsumer<T> channel);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool TryReceive<T>(
            [Guard.NotNegative] TimeSpan timeout,
            [Guard.NotNull] IConsumer<T> channel, 
            out T value);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        int ReceiveAny(
            out object value, 
            [Guard.NotEmpty] params IChannel[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int ReceiveAny<T>(
            out T value, 
            [Guard.NotEmpty] params IConsumer<T>[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int TryReceiveAny(
            [Guard.NotNegative] TimeSpan timeout, 
            out object value, 
            [Guard.NotEmpty] params IChannel[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPromiseBuilder<int> TryReceiveAnyAsync(
            [Guard.NotNegative] TimeSpan timeout, 
            out object value, 
            [Guard.NotEmpty] params IChannel[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int TryReceiveAny<T>(
            [Guard.NotNegative] TimeSpan timeout, 
            out T value, 
            [Guard.NotEmpty] params IConsumer<T>[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void Go(
            [Guard.NotNull] Action routine);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IPromiseBuilder Defer(
            [Guard.NotNull] 
            Action routine,
            [Guard.NotNegative, Guard.OrNull] 
            TimeSpan? delayBy = null,
            [Guard.NotPast, Guard.OrNull] 
            DateTime? delayUntilUtc = null,
            [Guard.NotNegative, Guard.OrNull] 
            TimeSpan? deadlineDuration = null,
            [Guard.NotPast, Guard.OrNull] 
            DateTime? deadlineUtc = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IPromiseBuilder<T> Defer<T>(
            [Guard.NotNull] Func<T> routine);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IPromiseBuilder<T> Defer<T>(
            [Guard.NotNegative] TimeSpan delay,
            [Guard.NotNull] Func<T> routine);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IScheduler Named(
            [Guard.NotEmpty] string name);
    }
}
