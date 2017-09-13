using NWheels.Kernel.Api.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices.Primitives
{
    public class StateMachineTests
    {
        [Fact]
        public void NewInstance_InitialState()
        {
            //-- Arrange, Act

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                new PhilisopherCodeBehind(), 
                null);

            //-- Assert

            Assert.Equal(machine.CurrentState, PhilisopherState.Thinking);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ReceiveTrigger_TransitionState()
        {
            //-- Arrange

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                new PhilisopherCodeBehind(),
                null);

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var state1 = machine.CurrentState;

            machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            var state2 = machine.CurrentState;

            //-- Assert

            Assert.Equal(state1, PhilisopherState.AcquiringForks);
            Assert.Equal(state2, PhilisopherState.Eating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_InvokeEventHandlers()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();

            //-- Act

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                null);
            var log1 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var log2 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            var log3 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilisopherTrigger.Full);
            var log4 = codeBehind.TakeLog();

            //-- Assert

            Assert.Equal(log1, new[] {
                "ThinkingEntered(to[Thinking])",
            });

            Assert.Equal(log2, new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
            });

            Assert.Equal(log3, new[] {
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])",
            });

            Assert.Equal(log4, new[] {
                "EatingLeaving(from[Eating]to[Thinking]by[Full])",
                "TransitioningFromEatingToThinking(from[Eating]to[Thinking]by[Full])",
                "ThinkingEntered(from[Eating]to[Thinking]by[Full])",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_OnLeavingThrows_CancelTransition()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents()
            {
                ThrowFromThinkingLeaving = true
            };

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                null);

            //-- Act

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            });

            //-- Assert

            Assert.Equal(exception.Message, "ThinkingLeaving");
            Assert.Equal(machine.CurrentState, PhilisopherState.Thinking);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "ThinkingEntered(to[Thinking])",
                "THROWING-FROM:ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_OnTransitioningThrows_EnterOriginStateBack()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents()
            {
                ThrowFromTransitioningFromAcquiringForksToEating = true
            };

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                null);

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            codeBehind.TakeLog();

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            });

            //-- Assert

            Assert.Equal(exception.Message, "TransitioningFromAcquiringForksToEating");
            Assert.Equal(machine.CurrentState, PhilisopherState.AcquiringForks);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "THROWING-FROM:TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "AcquiringForksEntered(from[AcquiringForks]to[Eating]by[GotForks])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_OnEnterThrows_StayInDestinationState()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents()
            {
                ThrowFromAcquiringForksEntered = true
            };

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                null);

            codeBehind.TakeLog();

            //-- Act

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            });

            //-- Assert

            Assert.Equal(exception.Message, "AcquiringForksEntered");
            Assert.Equal(machine.CurrentState, PhilisopherState.AcquiringForks);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "THROWING-FROM:AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_OnEnteredSendsFeedback_TransitionFurtherByFeedback()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents()
            {
                FeedBackFromEating = true
            };

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                null);

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            codeBehind.TakeLog();

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.GotForks); // will receive feedback Full from EatingEntered

            //-- Assert

            Assert.Equal(machine.CurrentState, PhilisopherState.Thinking);
            Assert.Equal(codeBehind.TakeLog(), new[] {
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingLeaving(from[Eating]to[Thinking]by[Full])",
                "TransitioningFromEatingToThinking(from[Eating]to[Thinking]by[Full])",
                "ThinkingEntered(from[Eating]to[Thinking]by[Full])",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PhilisopherState
        {
            Thinking,
            AcquiringForks,
            Eating
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PhilisopherTrigger
        {
            Hungry,
            GotForks,
            Deadlocked,
            Full
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class PhilisopherCodeBehind : IStateMachineCodeBehind<PhilisopherState, PhilisopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilisopherState, PhilisopherTrigger> machine)
            {
                machine.State(PhilisopherState.Thinking)
                    .SetAsInitial()
                    .OnTrigger(PhilisopherTrigger.Hungry).TransitionTo(PhilisopherState.AcquiringForks);

                machine.State(PhilisopherState.AcquiringForks)
                    .OnTrigger(PhilisopherTrigger.GotForks).TransitionTo(PhilisopherState.Eating)
                    .OnTrigger(PhilisopherTrigger.Deadlocked).TransitionTo(PhilisopherState.Thinking);

                machine.State(PhilisopherState.Eating)
                    .OnTrigger(PhilisopherTrigger.Full).TransitionTo(PhilisopherState.Thinking);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class PhilisopherCodeBehindWithEvents : IStateMachineCodeBehind<PhilisopherState, PhilisopherTrigger>
        {
            private readonly List<string> _log = new List<string>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<PhilisopherState, PhilisopherTrigger> machine)
            {
                machine.State(PhilisopherState.Thinking)
                    .SetAsInitial()
                    .OnEntered(ThinkingEntered)
                    .OnLeaving(ThinkingLeaving)
                    .OnTrigger(PhilisopherTrigger.Hungry).TransitionTo(PhilisopherState.AcquiringForks, TransitioningFromThinkingToAcquiringForks);

                machine.State(PhilisopherState.AcquiringForks)
                    .OnEntered(AcquiringForksEntered)
                    .OnLeaving(AcquiringForksLeaving)
                    .OnTrigger(PhilisopherTrigger.GotForks).TransitionTo(PhilisopherState.Eating, TransitioningFromAcquiringForksToEating)
                    .OnTrigger(PhilisopherTrigger.Deadlocked).TransitionTo(PhilisopherState.Thinking, TransitioningFromAcquiringForksToThinking);

                machine.State(PhilisopherState.Eating)
                    .OnEntered(EatingEntered)
                    .OnLeaving(EatingLeaving)
                    .OnTrigger(PhilisopherTrigger.Full).TransitionTo(PhilisopherState.Thinking, TransitioningFromEatingToThinking);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] TakeLog()
            {
                var result = _log.ToArray();
                _log.Clear();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ThrowFromThinkingEntered { get; set; }
            public bool ThrowFromThinkingLeaving { get; set; }
            public bool ThrowFromAcquiringForksEntered { get; set; }
            public bool ThrowFromAcquiringForksLeaving { get; set; }
            public bool ThrowFromTransitioningFromThinkingToAcquiringForks { get; set; }
            public bool ThrowFromTransitioningFromAcquiringForksToEating { get; set; }
            public bool ThrowFromTransitioningFromAcquiringForksToThinking { get; set; }
            public bool ThrowFromEatingEntered { get; set; }
            public bool ThrowFromEatingLeaving { get; set; }
            public bool ThrowFromTransitioningFromEatingToThinking { get; set; }
            public bool FeedBackFromEating { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LogAndThrow(
                bool shouldThrow,
                StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> args,
                [CallerMemberName] string methodName = null)
            {
                _log.Add(
                    (shouldThrow ? "THROWING-FROM:" : "") +
                    methodName +
                    "(" +
                    (args.HasFromState ? "from[" + args.FromState + "]" : "") +
                    (args.HasToState ? "to[" + args.ToState + "]" : "") +
                    (args.HasTrigger ? "by[" + args.Trigger + "]" : "") +
                    (args.Context != null ? "with[" + args.Context + "]" : "") +
                    ")");

                if (shouldThrow)
                {
                    throw new TestCodeBehindException(methodName);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ThinkingEntered(object sender, StateMachineFeedbackEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromThinkingEntered, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ThinkingLeaving(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromThinkingLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AcquiringForksEntered(object sender, StateMachineFeedbackEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromAcquiringForksEntered, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AcquiringForksLeaving(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromAcquiringForksLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromThinkingToAcquiringForks(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromThinkingToAcquiringForks, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromAcquiringForksToEating(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromAcquiringForksToEating, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromAcquiringForksToThinking(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromAcquiringForksToThinking, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EatingEntered(object sender, StateMachineFeedbackEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromEatingEntered, e);

                if (FeedBackFromEating)
                {
                    e.ReceiveFeedback(PhilisopherTrigger.Full);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EatingLeaving(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromEatingLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromEatingToThinking(object sender, StateMachineEventArgs<PhilisopherState, PhilisopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromEatingToThinking, e);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestCodeBehindException : Exception
        {
            public TestCodeBehindException(string message)
                : base(message)
            {
            }
        }
    }
}
