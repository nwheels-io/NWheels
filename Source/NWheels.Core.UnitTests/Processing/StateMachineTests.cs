using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Core.Processing;
using NWheels.Processing;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.Processing
{
    [TestFixture]
    public class StateMachineTests : FrameworkTestClassBase
    {
        [Test]
        public void NewInstance_InitialState()
        {
            //-- Arrange, Act

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(new PhilisopherCodeBehind(), Resolve<IProcessingExceptions>());

            //-- Assert

            Assert.That(machine.CurrentState, Is.EqualTo(PhilisopherState.Thinking));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ReceiveTrigger_TransitionState()
        {
            //-- Arrange

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(new PhilisopherCodeBehind(), Resolve<IProcessingExceptions>());

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var state1 = machine.CurrentState;

            machine.ReceiveTrigger(PhilisopherTrigger.TookForks);
            var state2 = machine.CurrentState;

            //-- Assert

            Assert.That(state1, Is.EqualTo(PhilisopherState.TakingForks));
            Assert.That(state2, Is.EqualTo(PhilisopherState.Eating));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TransitionState_InvokeEventHandlers()
        {
            //-- Arrange

            var machine = new StateMachine<PhilisopherState, PhilisopherTrigger>(new PhilisopherCodeBehind(), Resolve<IProcessingExceptions>());

            //-- Act

            machine.ReceiveTrigger(PhilisopherTrigger.Hungry);
            var state1 = machine.CurrentState;

            machine.ReceiveTrigger(PhilisopherTrigger.TookForks);
            var state2 = machine.CurrentState;

            //-- Assert

            Assert.That(state1, Is.EqualTo(PhilisopherState.TakingForks));
            Assert.That(state2, Is.EqualTo(PhilisopherState.Eating));
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum PhilisopherState
        {
            Thinking,
            TakingForks,
            Eating
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum PhilisopherTrigger
        {
            Hungry,
            TookForks,
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
                    .OnTrigger(PhilisopherTrigger.Hungry).TransitionTo(PhilisopherState.TakingForks);

                machine.State(PhilisopherState.TakingForks)
                    .OnTrigger(PhilisopherTrigger.TookForks).TransitionTo(PhilisopherState.Eating)
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
                    //.OnEntered(ThinkingEntered)
                    .OnTrigger(PhilisopherTrigger.Hungry).TransitionTo(PhilisopherState.TakingForks);

                machine.State(PhilisopherState.TakingForks)
                    .OnTrigger(PhilisopherTrigger.TookForks).TransitionTo(PhilisopherState.Eating)
                    .OnTrigger(PhilisopherTrigger.Deadlocked).TransitionTo(PhilisopherState.Thinking);

                machine.State(PhilisopherState.Eating)
                    .OnTrigger(PhilisopherTrigger.Full).TransitionTo(PhilisopherState.Thinking);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string[] GetLog()
            {
                return _log.ToArray();
            }
        }
    }
}
