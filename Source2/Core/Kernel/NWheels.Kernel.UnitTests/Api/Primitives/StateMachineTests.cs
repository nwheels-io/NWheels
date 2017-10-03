using NWheels.Kernel.Api.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.UnitTests.Api.Primitives
{
    public class StateMachineTests
    {
        [Fact]
        public void NewInstance_InitialState()
        {
            //-- Arrange, Act

            var machine = StateMachine.CreateFrom(new PhilisopherCodeBehind());

            //-- Assert

            Assert.Equal(machine.CurrentState, PhilosopherState.Thinking);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ReceiveTrigger_TransitionState()
        {
            //-- Arrange

            var machine = StateMachine.CreateFrom(new PhilisopherCodeBehind());

            //-- Act

            machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            var state1 = machine.CurrentState;

            machine.ReceiveTrigger(PhilosopherTrigger.GotForks);
            var state2 = machine.CurrentState;

            //-- Assert

            Assert.Equal(state1, PhilosopherState.AcquiringForks);
            Assert.Equal(state2, PhilosopherState.Eating);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ReceiveTrigger_NotDefinedForCurrentState_Throw()
        {
            //-- Arrange

            var machine = StateMachine.CreateFrom(new PhilisopherCodeBehind());
            Action act = () => machine.ReceiveTrigger(PhilosopherTrigger.GotForks);

            //-- Act  & assert

            act.ShouldThrow<Exception>(); //TODO: verify correct exception
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Transition_DestinationStateNotDefined_Throw()
        {
            //-- Arrange

            var machine = StateMachine.CreateFrom(new MissingStateCodeBehind());

            Action act = () => {
                machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            };

            //-- Act  & assert

            act.ShouldThrow<StateMachineException>()
                .Where(exc => exc.Reason == nameof(StateMachineException.DestinationStateNotDefined));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_CurrentStateChangedRaised()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var machine = StateMachine.CreateFrom(codeBehind);

            machine.CurrentStateChanged += (sender, args) => {
                codeBehind.AddLog($"CurrentStateChanged:{machine.CurrentState}");
            };

            //-- Act

            machine.ReceiveTrigger(PhilosopherTrigger.Hungry);

            //-- Assert

            codeBehind.TakeLog().Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])",
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                "CurrentStateChanged:AcquiringForks"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void TransitionState_InvokeEventHandlers()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();

            //-- Act

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);
            var log1 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            var log2 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilosopherTrigger.GotForks);
            var log3 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilosopherTrigger.Full);
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

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                ThrowFromThinkingLeaving = true
            };

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            //-- Act

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            });

            //-- Assert

            Assert.Equal(exception.Message, "ThinkingLeaving");
            Assert.Equal(machine.CurrentState, PhilosopherState.Thinking);
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

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            //-- Act

            machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            codeBehind.TakeLog();

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilosopherTrigger.GotForks);
            });

            //-- Assert

            Assert.Equal(exception.Message, "TransitioningFromAcquiringForksToEating");
            Assert.Equal(machine.CurrentState, PhilosopherState.AcquiringForks);
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

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                ThrowFromAcquiringForksEntered = true
            };

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            codeBehind.TakeLog();

            //-- Act

            var exception = Assert.Throws<TestCodeBehindException>(() => {
                machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            });

            //-- Assert

            Assert.Equal(exception.Message, "AcquiringForksEntered");
            Assert.Equal(machine.CurrentState, PhilosopherState.AcquiringForks);
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

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                FeedBackFromEating = true
            };

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            machine.ReceiveTrigger(PhilosopherTrigger.Hungry);
            codeBehind.TakeLog();

            //-- Act

            machine.ReceiveTrigger(PhilosopherTrigger.GotForks); // will receive feedback Full from EatingEntered

            //-- Assert

            Assert.Equal(machine.CurrentState, PhilosopherState.Thinking);
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

        [Fact]
        public void InitialState_OnEnteredSendsFeedback_TransitionByFeedback()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                FeedBackFromThinking = true
            };

            //-- Act

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.AcquiringForks);
            codeBehind.TakeLog().Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])",
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RestoreState_RestoredStateEntered()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                RestoreStateTo = PhilosopherState.Eating
            };

            //-- Act

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.Eating);
            codeBehind.TakeLog().Should().Equal(new[] {
                "EatingEntered(to[Eating])",
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CodeBdehind_NoInitialStateDefined_Throw()
        {
            //-- Arrange

            Action act = () => new StateMachine<PhilosopherState, PhilosopherTrigger>(new NoInitialStateCodeBehind());

            //-- Act & Assert

            act.ShouldThrow<Exception>(); //TODO: verify correct exception
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CodeBdehind_ReferToStateTwice_SameStateObject()
        {
            //-- Arrange

            var codeBehind = new DoubleStateCodeBehind();

            //-- Act

            var machine = new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind);

            //-- Assert

            codeBehind.ThinkingState2.Should().NotBeNull();
            codeBehind.ThinkingState2.Should().BeSameAs(codeBehind.ThinkingState1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CodeBdehind_AttemptRedefineInitialState_Throw()
        {
            //-- Arrange

            Action act = () => new StateMachine<PhilosopherState, PhilosopherTrigger>(new RedefiningInitialStateCodeBehind());

            //-- Act & Assert

            act.ShouldThrow<Exception>(); //TODO: verify correct exception
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CodeBdehind_AttemptRedefineTriggerOnState_Throw()
        {
            //-- Arrange

            Action act = () => new StateMachine<PhilosopherState, PhilosopherTrigger>(new DoubleTriggerCodeBehind());

            //-- Act & Assert

            act.ShouldThrow<Exception>(); //TODO: verify correct exception
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PhilosopherState
        {
            Thinking,
            AcquiringForks,
            Eating,
            SomeBadState
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum PhilosopherTrigger
        {
            Hungry,
            GotForks,
            Deadlocked,
            Full,
            SomeBadTrigger
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class PhilisopherCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .SetAsInitial()
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Deadlocked).TransitionTo(PhilosopherState.Thinking);

                machine.State(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class PhilisopherCodeBehindWithEvents : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            private readonly List<string> _log = new List<string>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .SetAsInitial()
                    .OnEntered(ThinkingEntered)
                    .OnLeaving(ThinkingLeaving)
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks, TransitioningFromThinkingToAcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .OnEntered(AcquiringForksEntered)
                    .OnLeaving(AcquiringForksLeaving)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating, TransitioningFromAcquiringForksToEating)
                    .OnTrigger(PhilosopherTrigger.Deadlocked).TransitionTo(PhilosopherState.Thinking, TransitioningFromAcquiringForksToThinking);

                machine.State(PhilosopherState.Eating)
                    .OnEntered(EatingEntered)
                    .OnLeaving(EatingLeaving)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking, TransitioningFromEatingToThinking);

                if (RestoreStateTo.HasValue)
                {
                    machine.RestoreState(RestoreStateTo.Value);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddLog(string message)
            {
                _log.Add(message);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] TakeLog()
            {
                var result = _log.ToArray();
                _log.Clear();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PhilosopherState? RestoreStateTo { get; set; }
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
            public bool FeedBackFromThinking { get; set; }
            public bool FeedBackFromEating { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LogAndThrow(
                bool shouldThrow,
                StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> args,
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

            private void ThinkingEntered(object sender, StateMachineFeedbackEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromThinkingEntered, e);

                if (FeedBackFromThinking)
                {
                    e.ReceiveFeedback(PhilosopherTrigger.Hungry);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ThinkingLeaving(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromThinkingLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AcquiringForksEntered(object sender, StateMachineFeedbackEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromAcquiringForksEntered, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AcquiringForksLeaving(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromAcquiringForksLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromThinkingToAcquiringForks(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromThinkingToAcquiringForks, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromAcquiringForksToEating(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromAcquiringForksToEating, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromAcquiringForksToThinking(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromAcquiringForksToThinking, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EatingEntered(object sender, StateMachineFeedbackEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromEatingEntered, e);

                if (FeedBackFromEating)
                {
                    e.ReceiveFeedback(PhilosopherTrigger.Full);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void EatingLeaving(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromEatingLeaving, e);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void TransitioningFromEatingToThinking(object sender, StateMachineEventArgs<PhilosopherState, PhilosopherTrigger> e)
            {
                LogAndThrow(ThrowFromTransitioningFromEatingToThinking, e);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class NoInitialStateCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Deadlocked).TransitionTo(PhilosopherState.Thinking);

                machine.State(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking);

                // this should throw: not initial state was defined
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class RedefiningInitialStateCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .SetAsInitial()
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .SetAsInitial() // this should throw: attempt to define initial state twice
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Deadlocked).TransitionTo(PhilosopherState.Thinking);

                machine.State(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DoubleStateCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                ThinkingState1 = machine.State(PhilosopherState.Thinking);
                ThinkingState1
                    .SetAsInitial()
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Deadlocked).TransitionTo(PhilosopherState.Thinking);

                machine.State(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking);

                ThinkingState2 = machine.State(PhilosopherState.Thinking); // this should return the same state builder object defined earlier
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IStateMachineStateBuilder<PhilosopherState, PhilosopherTrigger> ThinkingState1 { get; private set; }
            public IStateMachineStateBuilder<PhilosopherState, PhilosopherTrigger> ThinkingState2 { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class DoubleTriggerCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .SetAsInitial()
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);

                machine.State(PhilosopherState.AcquiringForks)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.GotForks).TransitionTo(PhilosopherState.Thinking); // this should throw: redifinition of GotForks trigger

                machine.State(PhilosopherState.Eating)
                    .OnTrigger(PhilosopherTrigger.Full).TransitionTo(PhilosopherState.Thinking);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class MissingStateCodeBehind : IStateMachineCodeBehind<PhilosopherState, PhilosopherTrigger>
        {
            public void BuildStateMachine(IStateMachineBuilder<PhilosopherState, PhilosopherTrigger> machine)
            {
                machine.State(PhilosopherState.Thinking)
                    .SetAsInitial()
                    .OnTrigger(PhilosopherTrigger.Hungry).TransitionTo(PhilosopherState.AcquiringForks);
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
