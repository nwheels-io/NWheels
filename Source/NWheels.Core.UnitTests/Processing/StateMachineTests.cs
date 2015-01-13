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
    }
}
