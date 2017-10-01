using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Primitives;
using NWheels.Microservices.Api;

namespace NWheels.Microservices.Runtime
{
    public class DefaultMicroserviceStateCodeBehind : IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger>
    {
        private readonly MicroserviceStateMachineOptions _options;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DefaultMicroserviceStateCodeBehind(MicroserviceStateMachineOptions options)
        {
            _options = options;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void BuildStateMachine(IStateMachineBuilder<MicroserviceState, MicroserviceTrigger> machine)
        {
            machine.State(MicroserviceState.Source)
                .OnTrigger(MicroserviceTrigger.Configure).TransitionTo(MicroserviceState.Configuring);

            machine.State(MicroserviceState.Configuring)
                .OnEntered(ConfiguringEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Configured)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.Configured)
                .OnTrigger(MicroserviceTrigger.Compile).TransitionTo(MicroserviceState.Compiling);

            machine.State(MicroserviceState.Compiling)
                .OnEntered(CompilingEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.CompiledStopped)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.CompiledStopped)
                .OnEntered(CompiledStoppedEntered)
                .OnTrigger(MicroserviceTrigger.Load).TransitionTo(MicroserviceState.Loading);

            machine.State(MicroserviceState.Loading)
                .OnEntered(LoadingEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.Standby)
                .OnTrigger(MicroserviceTrigger.Activate).TransitionTo(MicroserviceState.Activating)
                .OnTrigger(MicroserviceTrigger.Unload).TransitionTo(MicroserviceState.Unloading);

            machine.State(MicroserviceState.Activating)
                .OnEntered(ActivatingEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Active)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.Active)
                .OnTrigger(MicroserviceTrigger.Deactivate).TransitionTo(MicroserviceState.Deactivating);

            machine.State(MicroserviceState.Deactivating)
                .OnEntered(DeactivatingEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.Standby)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.Unloading)
                .OnEntered(UnloadingEntered)
                .OnTrigger(MicroserviceTrigger.OK).TransitionTo(MicroserviceState.CompiledStopped)
                .OnTrigger(MicroserviceTrigger.Failed).TransitionTo(MicroserviceState.Faulted);

            machine.State(MicroserviceState.Faulted)
                .OnEntered(FaultedgEntered);

            var initialState = (_options.BootConfig.IsPrecompiledMode ? MicroserviceState.CompiledStopped : MicroserviceState.Source);
            machine.State(initialState).SetAsInitial();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ConfiguringEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnConfiguring != null)
            {
                var result = _options.OnConfiguring();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void CompilingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnCompiling != null)
            {
                var result = _options.OnCompiling();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void CompiledStoppedEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (e.HasFromState && e.FromState == MicroserviceState.Unloading && _options.OnUnloaded != null)
            {
                _options.OnUnloaded();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void LoadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnLoading != null)
            {
                var result = _options.OnLoading();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ActivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnActivating != null)
            {
                var result = _options.OnActivating();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void DeactivatingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnDeactivating != null)
            {
                var result = _options.OnDeactivating();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void UnloadingEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnUnloading != null)
            {
                var result = _options.OnUnloading();
                e.ReceiveFeedback(result);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void FaultedgEntered(object sender, StateMachineFeedbackEventArgs<MicroserviceState, MicroserviceTrigger> e)
        {
            if (_options.OnFaulted != null)
            {
                _options.OnFaulted();
            }
        }
    }
}
