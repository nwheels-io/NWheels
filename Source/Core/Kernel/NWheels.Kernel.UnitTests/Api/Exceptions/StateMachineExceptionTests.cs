using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Testability;
using Xunit;

namespace NWheels.Kernel.UnitTests.Api.Exceptions
{
    public class StateMachineExceptionTests : TestBase.UnitTest
    {
        public const string TestCodeBehindTypeFriendlyName = "NWheels.Kernel.UnitTests.Api.Exceptions.StateMachineExceptionTests.TestCodeBehind";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KeyValuePairs_InitialStateNotSet()
        {
            //-- arrange

            var exception = StateMachineException.InitialStateNotSet(typeof(TestCodeBehind));

            //-- act

            var keyValuePairStrings = KeyValuePairsToStringArray(exception.KeyValuePairs);

            //-- assert

            keyValuePairStrings.Should().Contain(new[] {
                $"{nameof(StateMachineException.CodeBehind)}={TestCodeBehindTypeFriendlyName}"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KeyValuePairs_InitialStateAlreadyDefined()
        {
            //-- arrange

            var exception = StateMachineException.InitialStateAlreadyDefined(typeof(TestCodeBehind), "S1", "S2");

            //-- act

            var keyValuePairStrings = KeyValuePairsToStringArray(exception.KeyValuePairs);

            //-- assert

            keyValuePairStrings.Should().Contain(new[] {
                $"{nameof(StateMachineException.CodeBehind)}={TestCodeBehindTypeFriendlyName}",
                $"{nameof(StateMachineException.InitialState)}=S1",
                $"{nameof(StateMachineException.AttemptedState)}=S2"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KeyValuePairs_TransitionAlreadyDefined()
        {
            //-- arrange

            var exception = StateMachineException.TransitionAlreadyDefined(typeof(TestCodeBehind), "S1", "T1");

            //-- act

            var keyValuePairStrings = KeyValuePairsToStringArray(exception.KeyValuePairs);

            //-- assert

            keyValuePairStrings.Should().Contain(new[] {
                $"{nameof(StateMachineException.CodeBehind)}={TestCodeBehindTypeFriendlyName}",
                $"{nameof(StateMachineException.State)}=S1",
                $"{nameof(StateMachineException.Trigger)}=T1"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KeyValuePairs_TriggetNotValidInCurrentState()
        {
            //-- arrange

            var exception = StateMachineException.TriggerNotValidInCurrentState(typeof(TestCodeBehind), "SX", "TX");

            //-- act

            var keyValuePairStrings = KeyValuePairsToStringArray(exception.KeyValuePairs);

            //-- assert

            keyValuePairStrings.Should().Contain(new[] {
                $"{nameof(StateMachineException.CodeBehind)}={TestCodeBehindTypeFriendlyName}",
                $"{nameof(StateMachineException.State)}=SX",
                $"{nameof(StateMachineException.Trigger)}=TX"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void KeyValuePairs_DestinationStateNotDefined()
        {
            //-- arrange

            var exception = StateMachineException.DestinationStateNotDefined(typeof(TestCodeBehind), "SX");

            //-- act

            var keyValuePairStrings = KeyValuePairsToStringArray(exception.KeyValuePairs);

            //-- assert

            keyValuePairStrings.Should().Contain(new[] {
                $"{nameof(StateMachineException.CodeBehind)}={TestCodeBehindTypeFriendlyName}",
                $"{nameof(StateMachineException.State)}=SX"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEnumerable<string> KeyValuePairsToStringArray(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            return pairs.Select(p => $"{p.Key}={p.Value ?? string.Empty}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestCodeBehind
        {
        }
    }
}
