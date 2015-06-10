using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NWheels.Processing;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing
{
    [TestFixture]
    public class TransientStateMachineTests : UnitTestBase
    {
        [Test]
        public void NewInstance_InitialState()
        {
            //-- Arrange, Act

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                new PhilisopherCodeBehind(), 
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            //-- Assert

            Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.Thinking));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ReceiveTrigger_TransitionState()
        {
            //-- Arrange

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                new PhilisopherCodeBehind(),
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var state1 = machine.CurrentState;

            machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            var state2 = machine.CurrentState;

            //-- Assert

            Assert.That(state1, Is.EqualTo(PhilisopherState.AcquiringForks));
            Assert.That(state2, Is.EqualTo(PhilisopherState.Eating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TransitionState_InvokeEventHandlers()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();

            //-- Act

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());
            var log1 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var log2 = codeBehind.TakeLog();
            
            machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            var log3 = codeBehind.TakeLog();

            machine.ReceiveTrigger(PhilisopherTrigger.Full);
            var log4 = codeBehind.TakeLog();

            //-- Assert

            Assert.That(log1, Is.EqualTo(new[] {
                "ThinkingEntered(to[Thinking])",
            }));

            Assert.That(log2, Is.EqualTo(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
            }));

            Assert.That(log3, Is.EqualTo(new[] {
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])",
            }));

            Assert.That(log4, Is.EqualTo(new[] {
                "EatingLeaving(from[Eating]to[Thinking]by[Full])",
                "TransitioningFromEatingToThinking(from[Eating]to[Thinking]by[Full])",
                "ThinkingEntered(from[Eating]to[Thinking]by[Full])",
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(TestCodeBehindException), ExpectedMessage = "ThinkingLeaving")]
        public void TransitionState_OnLeavingThrows_CancelTransition()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                ThrowFromThinkingLeaving = true
            };

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            //-- Act

            try
            {
                machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            }
            catch ( TestCodeBehindException )
            {
                //-- Assert

                Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.Thinking));
                Assert.That(codeBehind.TakeLog(), Is.EqualTo(new[] {
                    "ThinkingEntered(to[Thinking])",
                    "THROWING-FROM:ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                }));

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(TestCodeBehindException), ExpectedMessage = "TransitioningFromAcquiringForksToEating")]
        public void TransitionState_OnTransitioningThrows_EnterOriginStateBack()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                ThrowFromTransitioningFromAcquiringForksToEating = true
            };

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            codeBehind.TakeLog();

            try
            {
                machine.ReceiveTrigger(PhilisopherTrigger.GotForks);
            }
            catch ( TestCodeBehindException )
            {
                //-- Assert

                Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.AcquiringForks));
                Assert.That(codeBehind.TakeLog(), Is.EqualTo(new[] {
                    "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                    "THROWING-FROM:TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                    "AcquiringForksEntered(from[AcquiringForks]to[Eating]by[GotForks])"
                }));

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(TestCodeBehindException), ExpectedMessage = "AcquiringForksEntered")]
        public void TransitionState_OnEnterThrows_StayInDestinationState()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                ThrowFromAcquiringForksEntered = true
            };

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            codeBehind.TakeLog();

            //-- Act

            try
            {
                machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            }
            catch ( TestCodeBehindException )
            {
                //-- Assert

                Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.AcquiringForks));
                Assert.That(codeBehind.TakeLog(), Is.EqualTo(new[] {
                    "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                    "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                    "THROWING-FROM:AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                }));

                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TransitionState_OnEnteredSendsFeedback_TransitionFurtherByFeedback()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents() {
                FeedBackFromEating = true
            };

            var machine = new TransientStateMachine<PhilisopherState, PhilisopherTrigger>(
                codeBehind,
                ResolveAuto<TransientStateMachine<PhilisopherState, PhilisopherTrigger>.ILogger>());

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            codeBehind.TakeLog();

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.GotForks); // will receive feedback Full from EatingEntered

            //-- Assert

            Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.Thinking));
            Assert.That(codeBehind.TakeLog(), Is.EqualTo(new[] {
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingLeaving(from[Eating]to[Thinking]by[Full])",
                "TransitioningFromEatingToThinking(from[Eating]to[Thinking]by[Full])",
                "ThinkingEntered(from[Eating]to[Thinking]by[Full])",
            }));
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

                if ( shouldThrow )
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

                if ( FeedBackFromEating )
                {
                    e.ReceiveFeedack(PhilisopherTrigger.Full);
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
