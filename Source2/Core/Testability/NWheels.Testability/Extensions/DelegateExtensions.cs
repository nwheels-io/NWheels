using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;

namespace NWheels.Testability.Extensions
{
    public static class DelegateExtensions
    {
        public static TException ShouldThrowException<TException>(this Action action, string because = null)
            where TException : Exception
        {
            try
            {
                action();
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected {typeof(TException).FullName}{{reason}}, but no exception was thrown.");
            }
            catch (TException expected)
            {
                return expected;
            }
            catch (Exception unexpected)
            {
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected {typeof(TException).FullName}{{reason}}, but caught {unexpected.GetType().FullName} : {unexpected.Message}");
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static async Task<TException> ShouldThrowExceptionAsync<TException>(this Func<Task> action, string because = null)
            where TException : Exception
        {
            try
            {
                await action();
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected {typeof(TException).FullName}{{reason}}, but no exception was thrown.");
            }
            catch (TException expected)
            {
                return expected;
            }
            catch (Exception unexpected)
            {
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected {typeof(TException).FullName}{{reason}}, but caught {unexpected.GetType().FullName} : {unexpected.Message}");
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static async Task ShouldNotThrowAnyExceptionAsync(this Func<Task> action, string because = null)
        {
            try
            {
                await action();
            }
            catch (Exception unexpected)
            {
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected no exceptions{{reason}}, but caught {unexpected.GetType().FullName} : {unexpected.Message}.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static async Task ShouldNotThrowExceptionAsync<TException>(this Func<Task> action, string because = null)
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException unexpected)
            {
                Execute.Assertion.BecauseOf(because).FailWith(
                    $"Expected no {typeof(TException).FullName}{{reason}}, but caught {unexpected.GetType().FullName} : {unexpected.Message}");
            }
        }
    }
}
