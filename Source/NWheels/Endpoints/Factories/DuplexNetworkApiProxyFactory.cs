using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Concurrency;
using NWheels.Concurrency.Core;
using NWheels.Endpoints.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Processing.Commands;
using NWheels.Processing.Commands.Factories;
using NWheels.Serialization;
using TT = Hapil.TypeTemplate;

namespace NWheels.Endpoints.Factories
{
    public class DuplexNetworkApiProxyFactory : ConventionObjectFactory, IDuplexNetworkApiProxyFactory
    {
        private readonly IComponentContext _components;
        private readonly CompactSerializer _serializer;
        private readonly IMethodCallObjectFactory _callFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DuplexNetworkApiProxyFactory(
            IComponentContext components, 
            DynamicModule module, 
            CompactSerializer serializer, 
            IMethodCallObjectFactory callFactory)
            : base(module)
        {
            _components = components;
            _serializer = serializer;
            _callFactory = callFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetOrBuildProxyType(Type remoteApiContract, Type localApiContract)
        {
            var typeEntry = GetOrBuildTypeEntry(remoteApiContract, localApiContract);
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateProxyInstance(Type remoteApiContract, Type localApiContract, IDuplexNetworkEndpointTransport transport, object localServer)
        {
            var typeEntry = GetOrBuildTypeEntry(remoteApiContract, localApiContract);

            var proxy = typeEntry.CreateInstance<
                object,
                IComponentContext, 
                CompactSerializer, 
                IDuplexNetworkEndpointTransport, 
                IMethodCallObjectFactory, 
                object
            >(0, _components, _serializer, transport, _callFactory, localServer);

            return proxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TRemoteApi CreateProxyInstance<TRemoteApi, TLocalApi>(IDuplexNetworkEndpointTransport transport, TLocalApi localServer)
        {
            var typeEntry = GetOrBuildTypeEntry(typeof(TRemoteApi), typeof(TLocalApi));
            
            var proxy = typeEntry.CreateInstance<
                TRemoteApi,
                IComponentContext,
                CompactSerializer,
                IDuplexNetworkEndpointTransport,
                IMethodCallObjectFactory,
                object
            >(0, _components, _serializer, transport, _callFactory, localServer);

            return proxy;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new ProxyConvention(context.TypeKey)
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeEntry GetOrBuildTypeEntry(Type remoteApiContract, Type localApiContract)
        {
            var typeKey = new TypeKey(primaryInterface: remoteApiContract, secondaryInterfaces: new[] { localApiContract });
            var typeEntry = base.GetOrBuildType(typeKey);
            return typeEntry;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ProxyBase
        {
            private readonly IDuplexNetworkApiProxyLogger _logger;
            private readonly ConcurrentDictionary<long, IMethodCallObject> _outstandingCallsByCorrelationId;
            private long _lastCorrelationId;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ProxyBase(IDuplexNetworkApiProxyLogger logger)
            {
                _logger = logger;
                _outstandingCallsByCorrelationId = new ConcurrentDictionary<long, IMethodCallObject>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise<T> RegisterOutstandingCallAsPromise<T>(IMethodCallObject call)
            {
                var correlationId = Interlocked.Increment(ref _lastCorrelationId);
                call.CorrelationId = correlationId;

                _logger.RegisteringOutstandingCall(call, correlationId);

                var added = _outstandingCallsByCorrelationId.TryAdd(correlationId, call);
                Debug.Assert(added);
                
                return new Promise<T>((IDeferred<T>)call);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Promise RegisterOutstandingCallAsPromise(IMethodCallObject call)
            {
                var correlationId = Interlocked.Increment(ref _lastCorrelationId);
                call.CorrelationId = correlationId;

                _logger.RegisteringOutstandingCall(call, correlationId);

                var added = _outstandingCallsByCorrelationId.TryAdd(correlationId, call);
                Debug.Assert(added);

                return new Promise((IDeferred)call);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<T> RegisterOutstandingCallAsTask<T>(IMethodCallObject call)
            {
                var correlationId = Interlocked.Increment(ref _lastCorrelationId);
                call.CorrelationId = correlationId;

                _logger.RegisteringOutstandingCall(call, correlationId);

                var added = _outstandingCallsByCorrelationId.TryAdd(correlationId, call);
                Debug.Assert(added);

                return ((TaskBasedDeferred<T>)call).Task;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task RegisterOutstandingCallAsTask(IMethodCallObject call)
            {
                var correlationId = Interlocked.Increment(ref _lastCorrelationId);
                call.CorrelationId = correlationId;

                _logger.RegisteringOutstandingCall(call, correlationId);

                var added = _outstandingCallsByCorrelationId.TryAdd(correlationId, call);
                Debug.Assert(added);

                return ((TaskBasedDeferred)call).Task;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected IMethodCallObject HandleReturnMessage(long correlationId, CompactDeserializationContext deserializationContext)
            {
                IMethodCallObject call;
                var found = _outstandingCallsByCorrelationId.TryRemove(correlationId, out call);

                if (!found)
                {
                    _logger.OutstandingCallNotFound(correlationId);
                    return null;
                }

                _logger.ReceivedReply(call, correlationId);

                call.Serializer.DeserializeOutput(deserializationContext);

                ThreadPool.UnsafeQueueUserWorkItem(_s_resolveDeferredCallOnCurrentThread, call);
                //((IAnyDeferred)call).Resolve(call.Result);

                return call;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected void HandleFaultMessage(long correlationId, string faultCode)
            {
                IMethodCallObject call;
                var found = _outstandingCallsByCorrelationId.TryRemove(correlationId, out call);

                if (!found)
                {
                    _logger.OutstandingCallNotFound(correlationId);
                    return;
                }

                _logger.ReceivedFault(call, correlationId, faultCode);

                ThreadPool.UnsafeQueueUserWorkItem(
                    _s_failDeferredCallOnCurrentThread, 
                    new DeferredFault() {
                        Call = call,
                        FaultCode = faultCode
                    }); 
                    

                //_faultCode = faultCode;
                //((IAnyDeferred)call).Fail(new RemotePartyFaultException(faultCode));
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            protected IDuplexNetworkApiProxyLogger Logger
            {
                get { return _logger; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static readonly WaitCallback _s_resolveDeferredCallOnCurrentThread = ResolveDeferredCallOnCurrentThread;
            private static readonly WaitCallback _s_failDeferredCallOnCurrentThread = FailDeferredCallOnCurrentThread;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static void ResolveDeferredCallOnCurrentThread(object callObject)
            {
                IMethodCallObject call = (IMethodCallObject)callObject;
                ((IAnyDeferred)call).Resolve(call.Result);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static void FailDeferredCallOnCurrentThread(object deferredFaultObject)
            {
                DeferredFault fault = (DeferredFault)deferredFaultObject;
                ((IAnyDeferred)fault.Call).Fail(new RemotePartyFaultException(fault.FaultCode));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct DeferredFault
        {
            public IMethodCallObject Call;
            public string FaultCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class ProxyBase<TRemoteApi, TLocalApi> : ProxyBase, IDuplexNetworkEndpointApiProxy
        {
            private readonly IComponentContext _components;
            private readonly CompactSerializer _serializer;
            private readonly IDuplexNetworkEndpointTransport _transport;
            private readonly IMethodCallObjectFactory _callFactory;
            private readonly object _localServer;
            private readonly CompactSerializerDictionary _dictionary;
            private readonly CompactRpcProtocol.HandleReturnMessageCallback _handleReturnMessageCallback;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ProxyBase(
                IComponentContext components, 
                CompactSerializer serializer, 
                IDuplexNetworkEndpointTransport transport, 
                IMethodCallObjectFactory callFactory,
                object localServer)
                : base(components.Resolve<IDuplexNetworkApiProxyLogger>())
            {
                _components = components;
                _serializer = serializer;
                _transport = transport;
                _callFactory = callFactory;
                _localServer = localServer;
                _dictionary = new StaticCompactSerializerDictionary();
                _handleReturnMessageCallback = this.HandleReturnMessage;

                // ReSharper disable once DoNotCallOverridableMethodsInConstructor
                OnRegisteringApiContracts(_dictionary);

                _transport.BytesReceived += OnTransportBytesReceived;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                if (Disposing != null)
                {
                    Disposing();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract void OnRegisteringApiContracts(CompactSerializerDictionary dictionary);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendCall(IMethodCallObject call)
            {
                using (var buffer = new MemoryStream())
                {
                    using (var writer = new CompactBinaryWriter(buffer))
                    {
                        base.Logger.SendingCall(call, call.CorrelationId);

                        var context = new CompactSerializationContext(_serializer, _dictionary, writer);
                        CompactRpcProtocol.WriteCall(call, context);
                        writer.Flush();

                        _transport.SendBytes(buffer.ToArray());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SendReply(IMethodCallObject call, Exception failure)
            {
                using (var buffer = new MemoryStream())
                {
                    using (var writer = new CompactBinaryWriter(buffer))
                    {
                        var context = new CompactSerializationContext(_serializer, _dictionary, writer);

                        if (failure == null)
                        {
                            base.Logger.SendingReply(call, call.CorrelationId);
                            CompactRpcProtocol.WriteReturn(call, context);
                        }
                        else
                        {
                            var fault = failure as IFaultException;
                            var faultCode = (
                                fault != null 
                                ? fault.GetQualifiedFaultCode() 
                                : CompactRpcProtocol.FaultCodeInternalError);

                            base.Logger.SendingFault(call, call.CorrelationId, faultCode);

                            CompactRpcProtocol.WriteFault(call, context, faultCode);
                        }

                        writer.Flush();
                        _transport.SendBytes(buffer.ToArray());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IDuplexNetworkEndpointApiProxy.NotifySessionClosed(ConnectionCloseReason reason)
            {
                var apiEvents = _localServer as IDuplexNetworkApiEvents;

                if (apiEvents != null)
                {
                    apiEvents.OnSessionClosed(this, reason);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IDuplexNetworkEndpointTransport IDuplexNetworkEndpointApiProxy.Transport
            {
                get { return _transport; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IDuplexNetworkEndpointApiProxy.RemoteContract
            {
                get { return typeof(TRemoteApi); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            Type IDuplexNetworkEndpointApiProxy.LocalContract
            {
                get { return typeof(TLocalApi); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object IDuplexNetworkEndpointApiProxy.LocalServer
            {
                get { return _localServer; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public event Action Disposing;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void OnTransportBytesReceived(byte[] bytes)
            {
                IMethodCallObject call;
                object returnValue;
                string faultCode;
                long correlationId;
                var messageType = ReadRpcMessage(bytes, out call, out returnValue, out faultCode, out correlationId);

                switch (messageType)
                {
                    case CompactRpcProtocol.RpcMessageType.Call:
                        HandleCallMessage(call);
                        break;
                    case CompactRpcProtocol.RpcMessageType.Return:
                        //HandleReturnMessage was already invoked from within ReadRpcMessage
                        //HandleReturnMessage(correlationId, returnValue);
                        break;
                    case CompactRpcProtocol.RpcMessageType.Fault:
                        HandleFaultMessage(correlationId, faultCode);
                        break;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void HandleCallMessage(IMethodCallObject call)
            {
                Exception failure = null;

                base.Logger.ReceivedCall(call, call.CorrelationId);

                try
                {
                    using (DuplexNetworkApi.CurrentCall.UseClientProxy(this))
                    {
                        call.ExecuteOn(_localServer);
                    }
                }
                catch (Exception e)
                {
                    failure = e;
                    //throw;
                }
                finally
                {
                    if (call.CorrelationId != CompactRpcProtocol.NoCorrelationId)
                    {
                        var deferred = (IAnyDeferred)call;
                        
                        if (deferred.IsResolved)
                        {
                            SendReply(call, failure);
                        }
                        else
                        {
                            var task = call.Result as Task;
                            var promise = call.Result as IAnyPromise;

                            if (task != null)
                            {
                                task.ContinueWith(t => SendReplyOnceCallCompleted(call));
                            }
                            else if (promise != null)
                            {
                                promise.Configure(continuation: () => SendReplyOnceCallCompleted(call));
                            }

                            //TODO: change as necessary to avoid allocation of closure object
                            //deferred.Configure(continuation: () => SendReplyOnceCallCompleted(call));
                        }
                    }
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private CompactRpcProtocol.RpcMessageType ReadRpcMessage(
                byte[] bytes, 
                out IMethodCallObject call, 
                out object returnValue, 
                out string faultCode, 
                out long correlationId)
            {
                using (var buffer = new MemoryStream(bytes))
                {
                    using (var reader = new CompactBinaryReader(buffer))
                    {
                        var context = new CompactDeserializationContext(_serializer, _dictionary, reader, _components);
                        var messageType = CompactRpcProtocol.ReadRpcMessage(
                            _callFactory, 
                            context, 
                            _handleReturnMessageCallback,
                            out call, 
                            out returnValue, 
                            out faultCode, 
                            out correlationId);

                        return messageType;
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void SendReplyOnceCallCompleted(IMethodCallObject call)
            {
                //var error = ((IAnyDeferred)call).Error;
                var error = ExtractCallCompletionError(call);
                SendReply(call, error);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Exception ExtractCallCompletionError(IMethodCallObject call)
            {
                var error = ((IAnyDeferred)call).Error;

                if (error == null)
                {
                    var task = call.Result as Task;
                    var promise = call.Result as IAnyPromise;

                    if (task != null)
                    {
                        error = task.Exception;
                    }
                    else if (promise != null)
                    {
                        error = promise.Error;
                    }
                }

                var aggregateException = (error as AggregateException);

                if (aggregateException != null)
                {
                    error = aggregateException.Flatten().InnerExceptions.FirstOrDefault();
                }

                var targetInvocationException = (error as TargetInvocationException);

                if (targetInvocationException != null)
                {
                    error = targetInvocationException.InnerException;
                }

                return error;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IDuplexNetworkApiProxyLogger : IApplicationEventLogger
        {
            [LogVerbose(ToPlainLog = true)]
            void SendingCall(IMethodCallObject call, long correlationId);
            
            [LogVerbose(ToPlainLog = true)]
            void SendingReply(IMethodCallObject call, long correlationId);
            
            [LogWarning(ToPlainLog = true)]
            void SendingFault(IMethodCallObject call, long correlationId, string faultCode);
            
            [LogVerbose(ToPlainLog = true)]
            void ReceivedCall(IMethodCallObject call, long correlationId);
            
            [LogVerbose(ToPlainLog = true)]
            void ReceivedReply(IMethodCallObject call, long correlationId);
            
            [LogWarning(ToPlainLog = true)]
            void ReceivedFault(IMethodCallObject call, long correlationId, string faultCode);
            
            [LogVerbose(ToPlainLog = true)]
            void RegisteringOutstandingCall(IMethodCallObject call, long correlationId);
            
            [LogError(ToPlainLog = true)]
            void OutstandingCallNotFound(long correlationId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class ProxyConvention : ImplementationConvention
        {
            private readonly Type _remoteApiContract;
            private readonly Type _localApiContract;
            private readonly DuplexNetworkApi.ContractDescription _remoteApiDescription;
            private readonly DuplexNetworkApi.ContractDescription _localApiDescription;
            private readonly Type _baseType;
            private Field<IDuplexNetworkEndpointTransport> _transportField;
            private Field<IMethodCallObjectFactory> _callFactoryField;
            private Field<object> _localServerField;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProxyConvention(TypeKey typeKey)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _remoteApiContract = typeKey.PrimaryInterface;
                _localApiContract = typeKey.SecondaryInterfaces[0];
                _baseType = typeof(ProxyBase<,>).MakeGenericType(_remoteApiContract, _localApiContract);

                _remoteApiDescription = new DuplexNetworkApi.ContractDescription(_remoteApiContract);
                _localApiDescription = new DuplexNetworkApi.ContractDescription(_localApiContract);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.BaseType = _baseType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                using (TT.CreateScope<TT.TContract, TT.TContract2>(_remoteApiContract, _localApiContract))
                {
                    writer
                        .Field("_transport", out _transportField)
                        .Field("_callFactory", out _callFactoryField)
                        .Field("_localServer", out _localServerField)
                        .Constructor<IComponentContext, CompactSerializer, IDuplexNetworkEndpointTransport, IMethodCallObjectFactory, object>(
                            (cw, components, serializer, transport, callFactory, localServer) => {
                                cw.Base(components, serializer, transport, callFactory, localServer);
                                _transportField.Assign(transport);
                                _callFactoryField.Assign(callFactory);
                                _localServerField.Assign(localServer);
                            });
                    
                    writer.ImplementBase<ProxyBase<TT.TContract, TT.TContract2>>()
                        .Method<CompactSerializerDictionary>(x => x.OnRegisteringApiContracts).Implement(WriteApiContractRegistrations);

                    writer.ImplementInterface<TT.TContract>()
                        .AllMethods(where: m => _remoteApiDescription.Methods.ContainsKey(m)).Implement(WriteRemoteContractMethod)
                        .AllEvents(where: e => _remoteApiDescription.Events.ContainsKey(e)).ImplementAutomatic(); 
                    
                    //TODO: generate local contract event handlers; the handlers will send messages to the remote party.
                    //TODO: raise remote contract events when corresponding messages arrive from the remote party
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteApiContractRegistrations(VoidMethodWriter writer, Argument<CompactSerializerDictionary> dictionary)
            {
                var contractsInAbcOrder = new[] { _remoteApiContract, _localApiContract }
                    .OrderBy(c => c.AssemblyQualifiedNameNonVersioned())
                    .ToArray();

                for (int i = 0 ; i < contractsInAbcOrder.Length ; i++)
                {
                    dictionary.Void<Type>(x => x.RegisterApiContract, writer.Const(contractsInAbcOrder[i]));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteRemoteContractMethod(TemplateMethodWriter writer)
            {
                var description = _remoteApiDescription.Methods[writer.OwnerMethod.MethodDeclaration];
                var callLocal = writer.Local<IMethodCallObject>();
                
                callLocal.Assign(
                    _callFactoryField.Func<MethodInfo, IMethodCallObject>(x => x.NewMessageCallObject, 
                        writer.Const(writer.OwnerMethod.MethodDeclaration)));

                writer.ForEachArgument(
                    (arg, index) => {
                        callLocal.Void(x => x.SetParameterValue, writer.Const(index), arg.CastTo<object>());
                    });

                if (description.IsOneWay)
                {
                    callLocal.Prop(x => x.CorrelationId).Assign(CompactRpcProtocol.NoCorrelationId);
                }
                else
                {
                    WriteRegisterOutstandingCall(writer, description, callLocal);
                }

                writer.This<ProxyBase<TT.TContract, TT.TContract2>>().Void(x => x.SendCall, callLocal);

                if (!description.IsOneWay)
                {
                    writer.Return(writer.ReturnValueLocal);
                }

                //if (!writer.OwnerMethod.IsVoid && typeof(IAnyPromise).IsAssignableFrom(writer.OwnerMethod.Signature.ReturnType))
                //{
                //    // we know that return type is Promise<T>
                //    var resultType = writer.OwnerMethod.Signature.ReturnType.GetGenericArguments()[0];

                //    using (TT.CreateScope<TT.TReturn, TT.TValue>(writer.OwnerMethod.Signature.ReturnType, resultType))
                //    {
                //        var deferredLocal = writer.Local<Deferred<TT.TValue>>(initialValue: writer.New<Deferred<TT.TValue>>());
                //        writer.This<ProxyBase<TT.TContract, TT.TContract2>>().Void(x => x.SetOustandingCall, deferredLocal);

                //        var promiseLocal = writer.Local<Promise<TT.TValue>>(initialValue: writer.New<Promise<TT.TValue>>(deferredLocal));

                //        writer.Return(promiseLocal.CastTo<TT.TReturn>());

                //        //writer.Return(writer.New<TT.TReturn>());
                //    }
                //}
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteRegisterOutstandingCall(
                TemplateMethodWriter writer, 
                DuplexNetworkApi.MemberDescription description, 
                Local<IMethodCallObject> callLocal)
            {
                writer.ReturnValueLocal = writer.Local<TT.TReturn>();

                if (description.PromiseResultType != null)
                {
                    using (TT.CreateScope<TT.TValue>(description.PromiseResultType))
                    {
                        if (description.PromiseTypeDefinition == typeof(Promise<>))
                        {
                            writer.ReturnValueLocal.Assign(
                                writer.This<ProxyBase>()
                                    .Func<IMethodCallObject, Promise<TT.TValue>>(x => x.RegisterOutstandingCallAsPromise<TT.TValue>, callLocal)
                                    .CastTo<TT.TReturn>());
                        }
                        else if (description.PromiseTypeDefinition == typeof(Task<>))
                        {
                            writer.ReturnValueLocal.Assign(
                                writer.This<ProxyBase>()
                                    .Func<IMethodCallObject, Task<TT.TValue>>(x => x.RegisterOutstandingCallAsTask<TT.TValue>, callLocal)
                                    .CastTo<TT.TReturn>());
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                }
                else
                {
                    if (description.PromiseType == typeof(Promise))
                    {
                        writer.ReturnValueLocal.Assign(
                            writer.This<ProxyBase>().Func<IMethodCallObject, Promise>(x => x.RegisterOutstandingCallAsPromise, callLocal).CastTo<TT.TReturn>());
                    }
                    else if (description.PromiseType == typeof(Task))
                    {
                        writer.ReturnValueLocal.Assign(
                            writer.This<ProxyBase>().Func<IMethodCallObject, Task>(x => x.RegisterOutstandingCallAsTask, callLocal).CastTo<TT.TReturn>());
                    }
                }
            }
        }
    }
}
