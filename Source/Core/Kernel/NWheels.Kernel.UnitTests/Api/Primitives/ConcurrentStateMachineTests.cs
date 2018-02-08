using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NWheels.Testability;
using NWheels.Kernel.Api.Primitives;
using Xunit;
using FluentAssertions;

using PhilosopherState = NWheels.Kernel.UnitTests.Api.Primitives.StateMachineTests.PhilosopherState;
using PhilosopherTrigger = NWheels.Kernel.UnitTests.Api.Primitives.StateMachineTests.PhilosopherTrigger;
using PhilisopherCodeBehindWithEvents = NWheels.Kernel.UnitTests.Api.Primitives.StateMachineTests.PhilisopherCodeBehindWithEvents;

namespace NWheels.Kernel.UnitTests.Api.Primitives
{
    public class ConcurrentStateMachineTests : TestBase.UnitTest
    {
        [Fact]
        public void RunOnCurrentThread_ExecuteReceivedTriggers()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var machine = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            machine.QueueTrigger(PhilosopherTrigger.Hungry);
            machine.QueueTrigger(PhilosopherTrigger.GotForks);

            var log0 = codeBehind.TakeLog();

            machine.RunOnCurrentThread(exitWhen: state => {
                return state == PhilosopherState.Eating;
            });

            var log1 = codeBehind.TakeLog();

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.Eating);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_ExecuteReceivedTriggers_WithContext()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var machine = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            machine.QueueTrigger(PhilosopherTrigger.Hungry, context: "CTX-A");
            machine.QueueTrigger(PhilosopherTrigger.GotForks, context: "CTX-B");

            var log0 = codeBehind.TakeLog();

            machine.RunOnCurrentThread(exitWhen: m => {
                return m == PhilosopherState.Eating;
            });

            var log1 = codeBehind.TakeLog();

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.Eating);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry]with[CTX-A])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry]with[CTX-A])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry]with[CTX-A])",
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks]with[CTX-B])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks]with[CTX-B])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks]with[CTX-B])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_OnErrorReturnsTrue_Continue()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var machine = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            var errorCount = 0;

            machine.QueueTrigger(PhilosopherTrigger.Hungry);
            machine.QueueTrigger(PhilosopherTrigger.Hungry); // error: Hungry trigger is invalid in AcquiringForks state
            machine.QueueTrigger(PhilosopherTrigger.GotForks);

            var log0 = codeBehind.TakeLog();

            machine.RunOnCurrentThread(
                exitWhen: state => state == PhilosopherState.Eating,
                onError: (state, err) => {
                    errorCount++;
                    codeBehind.AddLog("ERROR");
                    return true;
                });

            var log1 = codeBehind.TakeLog();

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.Eating);
            errorCount.Should().Be(1);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                "ERROR",
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_OnErrorReturnsFalse_Break()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var machine = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            var errorCount = 0;

            machine.QueueTrigger(PhilosopherTrigger.Hungry);
            machine.QueueTrigger(PhilosopherTrigger.Hungry); // error: Hungry trigger is invalid in AcquiringForks state
            machine.QueueTrigger(PhilosopherTrigger.GotForks);

            var log0 = codeBehind.TakeLog();

            machine.RunOnCurrentThread(
                exitWhen: state => state == PhilosopherState.Eating,
                onError: (state, err) => {
                    errorCount++;
                    codeBehind.AddLog("ERROR");
                    return false;
                });

            var log1 = codeBehind.TakeLog();

            //-- Assert

            machine.CurrentState.Should().Be(PhilosopherState.AcquiringForks);
            errorCount.Should().Be(1);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                "ERROR"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_OnErrorNoHandler_Continue()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var scheduler = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            scheduler.QueueTrigger(PhilosopherTrigger.Hungry);
            scheduler.QueueTrigger(PhilosopherTrigger.Hungry); // error: Hungry trigger is invalid in AcquiringForks state
            scheduler.QueueTrigger(PhilosopherTrigger.GotForks);

            var log0 = codeBehind.TakeLog();

            scheduler.RunOnCurrentThread(
                exitWhen: state => state == PhilosopherState.Eating,
                onError: null);

            var log1 = codeBehind.TakeLog();

            //-- Assert

            scheduler.CurrentState.Should().Be(PhilosopherState.Eating);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksLeaving(from[AcquiringForks]to[Eating]by[GotForks])",
                "TransitioningFromAcquiringForksToEating(from[AcquiringForks]to[Eating]by[GotForks])",
                "EatingEntered(from[AcquiringForks]to[Eating]by[GotForks])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_CancellationRequested_Break()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var scheduler = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));
            var cancellation = new CancellationTokenSource();

            scheduler.CurrentStateChanged += (sender, args) => {
                if (scheduler.CurrentState == PhilosopherState.AcquiringForks)
                {
                    cancellation.Cancel();
                }
            };

            //-- Act

            var errorCount = 0;

            scheduler.QueueTrigger(PhilosopherTrigger.Hungry);
            scheduler.QueueTrigger(PhilosopherTrigger.GotForks); // should not execute

            var log0 = codeBehind.TakeLog();

            scheduler.RunOnCurrentThread(
                exitWhen: state => state == PhilosopherState.Eating,
                onError: (state, err) => {
                    errorCount++;
                    codeBehind.AddLog("ERROR");
                    return true;
                },
                cancellation: cancellation.Token);

            var log1 = codeBehind.TakeLog();

            //-- Assert

            scheduler.CurrentState.Should().Be(PhilosopherState.AcquiringForks);
            errorCount.Should().Be(0);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunOnCurrentThread_ExitWhenReturnsFalse_Break()
        {
            //-- Arrange

            var codeBehind = new PhilisopherCodeBehindWithEvents();
            var scheduler = StateMachineScheduler.CreateFrom(new StateMachine<PhilosopherState, PhilosopherTrigger>(codeBehind));

            //-- Act

            var errorCount = 0;

            scheduler.QueueTrigger(PhilosopherTrigger.Hungry);
            scheduler.QueueTrigger(PhilosopherTrigger.GotForks); // should not execute

            var log0 = codeBehind.TakeLog();

            scheduler.RunOnCurrentThread(
                exitWhen: state => state == PhilosopherState.AcquiringForks,
                onError: (state, err) => {
                    errorCount++;
                    codeBehind.AddLog("ERROR");
                    return true;
                });

            var log1 = codeBehind.TakeLog();

            //-- Assert

            scheduler.CurrentState.Should().Be(PhilosopherState.AcquiringForks);
            errorCount.Should().Be(0);

            log0.Should().Equal(new[] {
                "ThinkingEntered(to[Thinking])"
            });

            log1.Should().Equal(new[] {
                "ThinkingLeaving(from[Thinking]to[AcquiringForks]by[Hungry])",
                "TransitioningFromThinkingToAcquiringForks(from[Thinking]to[AcquiringForks]by[Hungry])",
                "AcquiringForksEntered(from[Thinking]to[AcquiringForks]by[Hungry])"
            });
        }
    }
}
