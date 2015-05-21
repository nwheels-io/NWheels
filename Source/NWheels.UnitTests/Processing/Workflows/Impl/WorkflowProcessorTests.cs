using System;
using System.Collections.Generic;
using NUnit.Framework;
using NWheels.Extensions;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;
using NWheels.Processing.Workflows.Impl;
using NWheels.Testing;

namespace NWheels.UnitTests.Processing.Workflows.Impl
{
    [TestFixture]
    public class WorkflowProcessorTests : UnitTestBase
    {
        private List<string> _log;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanExecuteSingleActorWorkflow()
        {
            //-- arrange

            var environment = new ProcessorTestEnvironment(Framework);

            environment.AddActor<string>("A1", priority: 1, onExecute: LogActorExecute, onRoute: LogRouterRoute);
            environment.EndBuildWorkflow();
            environment.InitialWorkItem = "ABC";

            //-- act

            var result = environment.ProcessorUnderTest.Run();

            //-- assert

            Assert.That(result, Is.EqualTo(ProcessorResult.Completed));
            Assert.That(_log, Is.EqualTo(new[] { "A1.Execute(ABC)", "A1.Route()" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanExecuteWorkflowWithConditionalRouting()
        {
            //-- arrange

            var buildEnvironment = new Func<ProcessorTestEnvironment>(() => {
                var environment = new ProcessorTestEnvironment(Framework);
                
                environment.AddActor<int>("A1", 1,
                    onExecute: LogActorExecute,
                    onRoute: ctx => ctx.EnqueueWorkItem((ctx.GetActorWorkItem<int>() % 2) == 0 ? "E1" : "O1", ctx.GetActorWorkItem<int>()));
                
                environment.AddActor<int>("E1", 2, LogActorExecute, LogRouterRoute);
                environment.AddActor<int>("O1", 2, LogActorExecute, LogRouterRoute);
                environment.EndBuildWorkflow();
                
                return environment;
            });

            var environment1 = buildEnvironment();
            environment1.InitialWorkItem = 111;
            
            var environment2 = buildEnvironment();
            environment2.InitialWorkItem = 222;

            //-- act

            var result1 = environment1.ProcessorUnderTest.Run();
            var log1 = TakeLog();

            var result2 = environment2.ProcessorUnderTest.Run();
            var log2 = TakeLog();

            //-- assert

            Assert.That(result1, Is.EqualTo(ProcessorResult.Completed));
            Assert.That(log1, Is.EqualTo(new[] { "A1.Execute(111)", "O1.Execute(111)", "O1.Route()" }));

            Assert.That(result2, Is.EqualTo(ProcessorResult.Completed));
            Assert.That(log2, Is.EqualTo(new[] { "A1.Execute(222)", "E1.Execute(222)", "E1.Route()" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanPassWorkItemAndActorResultToRouter()
        {
            //-- arrange

            var environment = new ProcessorTestEnvironment(Framework);
            IWorkflowRouterContext routerContext = null;

            environment.AddActor<int>("A1", 1,
                onExecute: (ctx, n) => ctx.SetResult(TimeSpan.FromSeconds(n)),
                onRoute: ctx => routerContext = ctx);

            environment.EndBuildWorkflow();

            //-- act

            environment.InitialWorkItem = 123;
            var result = environment.ProcessorUnderTest.Run();

            //-- assert

            Assert.That(result, Is.EqualTo(ProcessorResult.Completed));
            
            Assert.That(routerContext.HasActorWorkItem<string>(), Is.False);
            Assert.That(routerContext.HasActorWorkItem<int>(), Is.True);
            Assert.That(routerContext.GetActorWorkItem<int>(), Is.EqualTo(123));

            Assert.That(routerContext.HasActorResult<int>(), Is.False);
            Assert.That(routerContext.HasActorResult<TimeSpan>(), Is.True);
            Assert.That(routerContext.GetActorResult<TimeSpan>(), Is.EqualTo(TimeSpan.FromSeconds(123)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSuspendAwaitingForSingleEvent()
        {
            //-- arrange

            var environment = ProcessorTestEnvironment.Build(Framework, env => env
                .AddActor<string>("A1", 1, LogActorExecute, onRoute: ctx => LogRouterAndRoute(ctx, routeToActor: "B1"))
                .AddActor<string>("B1", 2,
                    onExecute: (ctx, s) => {
                        LogActorExecute(ctx, s);
                        ctx.AwaitEvent<TestEvent, DayOfWeek>(DayOfWeek.Friday, TimeSpan.FromHours(1));
                    },
                    onRoute: ctx => {
                        LogRouterRoute(ctx);
                        ctx.EnqueueWorkItem("C1", ctx.GetReceivedEvent<TestEvent>().Payload);
                    })
                .AddActor<string>("C1", 3, LogActorExecute, LogRouterRoute));

            //-- act

            environment.InitialWorkItem = "ABC";
            var result = environment.ProcessorUnderTest.Run();

            //-- assert

            Assert.That(result, Is.EqualTo(ProcessorResult.Suspended));
            Assert.That(_log, Is.EqualTo(new[] { "A1.Execute(ABC)", "A1.Route()", "B1.Execute(ABC)" }));
            Assert.That(environment.AwaitEventRequests, Is.EqualTo(new[] {
                new ProcessorTestEnvironment.AwaitEventRequest(typeof(TestEvent), DayOfWeek.Friday, TimeSpan.FromHours(1))
            }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanDispatchEventAndResumeToCompletion()
        {
            //-- arrange

            var createEnvironment = ProcessorTestEnvironment.BuildFactory(Framework, env => env
                .AddActor<string>("A1", 1, LogActorExecute, onRoute: ctx => LogRouterAndRoute(ctx, routeToActor: "B1"))
                .AddActor<string>("B1", 2,
                    onExecute: (ctx, s) => {
                        LogActorExecute(ctx, s);
                        ctx.AwaitEvent<TestEvent, DayOfWeek>(DayOfWeek.Friday, TimeSpan.FromHours(1));
                    },
                    onRoute: ctx => {
                        LogRouterRoute(ctx);
                        ctx.EnqueueWorkItem("C1", ctx.GetReceivedEvent<TestEvent>().Payload);
                    })
                .AddActor<string>("C1", 3, LogActorExecute, LogRouterRoute));

            WorkflowProcessorSnapshot processorSnapshot;
            
            var firstRunResult = createEnvironment().Run("ABC", out processorSnapshot);
            var firstRunLog = TakeLog();

            //-- act

            var receivedEvent = new TestEvent(DayOfWeek.Friday, payload: "XYZ");
            var secondRunResult = createEnvironment().DispatchAndRun(ref processorSnapshot, receivedEvent);
            var secondRunLog = TakeLog();

            //-- assert

            Assert.That(firstRunResult, Is.EqualTo(ProcessorResult.Suspended));
            Assert.That(firstRunLog, Is.EqualTo(new[] { "A1.Execute(ABC)", "A1.Route()", "B1.Execute(ABC)" }));

            Assert.That(secondRunResult, Is.EqualTo(ProcessorResult.Completed));
            Assert.That(secondRunLog, Is.EqualTo(new[] { "B1.Route()", "C1.Execute(XYZ)", "C1.Route()" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAwaitForEventsOnMultiplePaths()
        {
            //-- arrange

            var joinCounter = 0;
            var createEnvironment = ProcessorTestEnvironment.BuildFactory(Framework, env => env
                #region Workflow Structure
                
                .AddActor<string>("A", 1, onExecute: LogActorExecute, onRoute: ctx => {
                    ctx.EnqueueWorkItem("B", ctx.GetActorWorkItem<string>() + "@B");
                    ctx.EnqueueWorkItem("C", ctx.GetActorWorkItem<string>() + "@C");
                    ctx.EnqueueWorkItem("D", ctx.GetActorWorkItem<string>() + "@D");
                })
                .AddActor<string>("B", 2,
                    onExecute: (ctx, s) => {
                        LogActorExecute(ctx, s);
                        ctx.AwaitEvent<TestEvent, DayOfWeek>(DayOfWeek.Monday, TimeSpan.FromHours(1));
                    },
                    onRoute: ctx => ctx.EnqueueWorkItem("J", ctx.GetReceivedEvent<TestEvent>().Payload))
                .AddActor<string>("C", 3,
                    onExecute: (ctx, s) => {
                        LogActorExecute(ctx, s);
                        ctx.AwaitEvent<TestEvent, DayOfWeek>(DayOfWeek.Tuesday, TimeSpan.FromHours(2));
                    },
                    onRoute: ctx => ctx.EnqueueWorkItem("J", ctx.GetReceivedEvent<TestEvent>().Payload))
                .AddActor<string>("D", 4,
                    onExecute: (ctx, s) => {
                        LogActorExecute(ctx, s);
                        ctx.AwaitEvent<TestEvent, DayOfWeek>(DayOfWeek.Wednesday, TimeSpan.FromHours(3));
                    },
                    onRoute: ctx => ctx.EnqueueWorkItem("J", ctx.GetReceivedEvent<TestEvent>().Payload))
                .AddActor<string>("J", 5,
                    onExecute: LogActorExecute,
                    onRoute: ctx => {
                        AddLog("J.Route[count={0}+1={1}]", joinCounter, joinCounter + 1);
                        if ( ++joinCounter == 3 )
                        {
                            ctx.EnqueueWorkItem("E", "DONE");
                        }
                    })
                .AddActor<string>("E", 6, LogActorExecute, LogRouterRoute)
                
                #endregion
            );

            // processor snapshot is encapsulated into an array in order to be safely passed among closures.
            WorkflowProcessorSnapshot processorSnapshot = null;//new WorkflowProcessorSnapshot[] { null }; 
            var runResults = new List<ProcessorResult>();
            var runLogs = new List<string[]>();

            Action<TestEvent> dispatchAndRun = e => {
                runResults.Add(createEnvironment().DispatchAndRun(ref processorSnapshot, e));
                runLogs.Add(TakeLog());
            };

            //-- act

            runResults.Add(createEnvironment().Run("TEST", out processorSnapshot));
            runLogs.Add(TakeLog());

            dispatchAndRun(new TestEvent(DayOfWeek.Tuesday, "CCC"));
            dispatchAndRun(new TestEvent(DayOfWeek.Wednesday, "DDD"));
            dispatchAndRun(new TestEvent(DayOfWeek.Monday, "BBB"));

            //-- assert

            Assert.That(runResults, Is.EqualTo(new[] {
                ProcessorResult.Suspended, 
                ProcessorResult.Suspended, 
                ProcessorResult.Suspended, 
                ProcessorResult.Completed
            }));

            Assert.That(runLogs[0], Is.EqualTo(new[] { "A.Execute(TEST)", "B.Execute(TEST@B)", "C.Execute(TEST@C)", "D.Execute(TEST@D)" }));
            Assert.That(runLogs[1], Is.EqualTo(new[] { "J.Execute(CCC)", "J.Route[count=0+1=1]" }));
            Assert.That(runLogs[2], Is.EqualTo(new[] { "J.Execute(DDD)", "J.Route[count=1+1=2]" }));
            Assert.That(runLogs[3], Is.EqualTo(new[] { "J.Execute(BBB)", "J.Route[count=2+1=3]", "E.Execute(DONE)", "E.Route()" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogActorExecute<TWorkItem>(IWorkflowActorContext context, TWorkItem workItem)
        {
            AddLog("{0}.Execute({1})", context.ActorName, workItem);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogRouterRoute(IWorkflowRouterContext context)
        {
            AddLog("{0}.Route()", context.ActorName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogRouterAndRoute(IWorkflowRouterContext context, string routeToActor)
        {
            LogRouterRoute(context);
            context.EnqueueWorkItem<object>(routeToActor, context.GetActorWorkItem<object>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddLog(string format, params object[] args)
        {
            _log.Add(format.FormatIf(args));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string[] TakeLog()
        {
            var result = _log.ToArray();
            _log.Clear();
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestEvent : WorkflowEventBase<DayOfWeek>
        {
            public TestEvent(DayOfWeek key, string payload)
                : base(key)
            {
                Payload = payload;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Payload { get; private set; }
        }
    }
}
