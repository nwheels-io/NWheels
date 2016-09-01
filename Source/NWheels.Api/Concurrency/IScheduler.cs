using System;

namespace NWheels.Api.Concurrency
{
    public interface IScheduler
    {
        [return: Guard.NotNull] IChannel<T> NewChannel<T>(
            [Guard.NotEmpty] string name);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        int Select(
            out object value, 
            [Guard.NotEmpty] params IChannel[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int Select<T>(
            out T value, 
            [Guard.NotEmpty] params IConsumer<T>[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int TrySelect(
            [Guard.NotNegative] TimeSpan timeout, 
            out object value, 
            [Guard.NotEmpty] params IChannel[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.GreaterOrEqual(-1)] 
        int TrySelect<T>(
            [Guard.NotNegative] TimeSpan timeout, 
            out T value, 
            [Guard.NotEmpty] params IConsumer<T>[] channels);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void Go(
            [Guard.NotNull] Action routine);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IPromiseBuilder Defer(
            [Guard.NotNull] Action routine,
            [Guard.NotNegative, Guard.OrNull] TimeSpan? delayBy = null,
            [Guard.NotNegative, Guard.OrNull] TimeSpan? deadlineDuration = null);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [return: Guard.NotNull] 
        IPromiseBuilder Defer(
            [Guard.NotNull] Action routine,
            [Guard.NotNull] DateTime? delayUntilUtc = null,
            [Guard.NotNegative, Guard.OrNull] DateTime? deadlineUtc = null);

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
