#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NWheels.UnitTests.Processing.Spikes
{
    [TestFixture]
    public class AsyncAwaitAssumptionTests
    {
        [Test]
        public void CanUseCompiledStateMachine()
        {
            var events = new EventSource();
            var workflow = new ApprovalWorkflow();

            workflow.Application = "ABC";

            PrintLog("Calling workflow.Run(). . .");

            workflow.Run(events);

            PrintLog("workflow.Run() returned.");
            PrintLog("workflow.ApprovalResponse=[{0}]", workflow.ApprovalResponse);

            PrintLog("Triggering awaited event. . .");

            events.Events.Dequeue().Fire(true);

            PrintLog("Fire() returned. . .");
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        private static void PrintLog(string format, params object[] args)
        {
            Console.WriteLine("thread {0,5} @ {1:HH:mm:ss.fff} : {2}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, string.Format(format, args));
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExternalEvent
        {
            void Fire(object data);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExternalEvent<TEventArgs> : IExternalEvent, INotifyCompletion
        {
            private Action _continuation;
            private TEventArgs _eventArgs;
            private bool _isCompleted;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ExternalEvent(string eventId)
            {
                PrintLog("$$ ExternalEvent[{0}].ctor()", eventId);
                this.EventId = eventId;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnCompleted(Action continuation)
            {
                PrintLog("$$ ExternalEvent[{0}].OnCompleted({1})", EventId, continuation);
                _continuation = continuation;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExternalEvent<TEventArgs> GetAwaiter()
            {
                PrintLog("$$ ExternalEvent[{0}].GetAwaiter()", EventId);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TEventArgs GetResult()
            {
                PrintLog("$$ ExternalEvent[{0}].GetResult()=[{1}]", EventId, _eventArgs);
                return _eventArgs;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsCompleted
            {
                get
                {
                    PrintLog("$$ ExternalEvent[{0}].IsCompleted=[{1}]", EventId, _isCompleted);
                    return _isCompleted;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Fire(object data)
            {
                PrintLog("$$ ExternalEvent[{0}].Fire(data=[{1}])", EventId, data);

                _eventArgs = (TEventArgs)data;
                _isCompleted = true;

                PrintLog("$$ ExternalEvent[{0}]: calling continuation action. . .");

                _continuation();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string EventId { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class AbstractWorkflow<T>
        {
            public abstract void Run(T input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IExternalServices
        {
            ExternalEvent<string> RequestExtraDocument(string documentId);
            ApprovalResult AutoApprove(Form12345C form);
            ExternalEvent<ApprovalResult> RequestManualApproval(Form12345C form);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExternalServices : IExternalServices
        {
            private int _autoApproveGate;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExternalEvent<string> RequestExtraDocuments(IEnumerable<string> documentId)
            {
                return new ExternalEvent<string>("REQ-DOC[ID=" + documentId + "]");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ApprovalResult AutoApprove(Form12345C form)
            {
                var value = Interlocked.Increment(ref _autoApproveGate);
                return ((value % 2) == 0 ? ApprovalResult.Approved : ApprovalResult.Rejected);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExternalEvent<ApprovalResult> RequestManualApproval(Form12345C form)
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Form12345C
        {
            public List<string> ExtraDocuments { get; set; }
            public ApprovalResult? Approval { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ExtraDocumentsRequired
            {
                get
                {
                    return (ExtraDocuments == null || ExtraDocuments.Count < 3);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerable<string> GetRequiredExtraDocuments()
            {
                var currentCount = (ExtraDocuments != null ? ExtraDocuments.Count : 0);

                for ( int i = currentCount + 1 ; i <= 3 ; i++ )
                {
                    yield return "DOC#" + i;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AddExtraDocuments(IEnumerable<string> documents)
            {
                if ( ExtraDocuments == null )
                {
                    ExtraDocuments = new List<string>();
                }

                ExtraDocuments.AddRange(documents);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetApproved()
            {
                this.Approval = ApprovalResult.Approved;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetRejected()
            {
                this.Approval = ApprovalResult.Rejected;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ProcessingWorkflowOfForm12345C : AbstractWorkflow<Form12345C>
        {
            private readonly IExternalServices _services;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessingWorkflowOfForm12345C(IExternalServices services)
            {
                _services = services;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override async void Run(Form12345C form)
            {
                while ( form.ExtraDocumentsRequired )
                {
                    var extraDocuments = await _services.RequestExtraDocuments()
                    form.AddExtraDocuments(extraDocuments);
                }

                var approvalResult = _services.AutoApprove(form);

                while ( true )
                {
                    switch ( approvalResult )
                    {
                        case ApprovalResult.Approved:
                            form.SetApproved();
                            return;
                        case ApprovalResult.Rejected:
                            form.SetRejected();
                            return;
                        case ApprovalResult.Manual:
                            approvalResult = await _services.RequestManualApproval(form); // can take days
                            break;
                    }
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public enum ApprovalResult
        {
            Approved,
            Rejected,
            Manual
        }
    }


    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EventSource
    {
        public EventSource()
        {
            this.Events = new Queue<IEventAwaiter>();
        }

        public EventAwaiter<TEventArgs> WaitForEvent<TEventArgs>(string eventId)
        {
            var awaiter = new EventAwaiter<TEventArgs>(eventId);
            this.Events.Enqueue(awaiter);

            PrintLog("$$ EventSource: awaiter enqueued for eventId=[{0}]", eventId);
            
            return awaiter;
        }

        public Queue<IEventAwaiter> Events { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IEventAwaiter
    {
        void Fire(object data);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EventAwaiter<TEventArgs> : IEventAwaiter, INotifyCompletion
    {
        private Action _continuation;
        private TEventArgs _eventArgs;
        private bool _isCompleted;

        public EventAwaiter(string eventId)
        {
            PrintLog("$$ EventAwaiter[{0}].ctor()", eventId);
            this.EventId = eventId;
        }

        public void OnCompleted(Action continuation)
        {
            PrintLog("$$ EventAwaiter[{0}].OnCompleted({1})", EventId, continuation);
            _continuation = continuation;
        }

        public EventAwaiter<TEventArgs> GetAwaiter()
        {
            PrintLog("$$ EventAwaiter[{0}].GetAwaiter()", EventId);
            return this;
        }

        public TEventArgs GetResult()
        {
            PrintLog("$$ EventAwaiter[{0}].GetResult()=[{1}]", EventId, _eventArgs);
            return _eventArgs;
        }

        public bool IsCompleted
        {
            get
            {
                PrintLog("$$ EventAwaiter[{0}].IsCompleted=[{1}]", EventId, _isCompleted);
                return _isCompleted;
            }
        }

        public void Fire(object data)
        {
            PrintLog("$$ EventAwaiter[{0}].Fire(data=[{1}])", EventId, data);

            _eventArgs = (TEventArgs)data;
            _isCompleted = true;
            
            PrintLog("$$ EventAwaiter[{0}]: calling continuation action. . .");

            _continuation();
        }

        public string EventId { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ApprovalWorkflow
    {
        public async void Run(EventSource events)
        {
            PrintLog("ApprovalWorkflow started: Application={0}", Application);

            ApprovalRequest = "REQ:" + this.Application;

            PrintLog("ApprovalRequest={0}", ApprovalRequest);
            
            PrintLog("Waiting for APPROVE_OR_REJECT . . .");

            WasApproved = await events.WaitForEvent<bool>("APPOVE_OR_REJECT");

            PrintLog("APPROVE_OR_REJECT received, WasApproved={0}", WasApproved);

            ApprovalResponse = (WasApproved ? "APPROVED" : "REJECTED");

            PrintLog("ApprovalResponse={0}", ApprovalResponse);

            PrintLog("ApprovalWorkflow finished: Application={0}", Application);
        }

        public string Application { get; set; }
        public string ApprovalRequest { get; set; }
        public string ApprovalResponse { get; set; }
        public bool WasApproved { get; set; }
    }

}

#endif