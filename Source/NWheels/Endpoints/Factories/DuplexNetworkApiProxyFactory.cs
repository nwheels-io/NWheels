using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Concurrency;
using NWheels.Concurrency.Core;
using NWheels.Endpoints.Core;
using NWheels.Extensions;
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

        public abstract class ProxyBase<TRemoteApi, TLocalApi> : IDuplexNetworkEndpointApiProxy
        {
            private readonly IComponentContext _components;
            private readonly CompactSerializer _serializer;
            private readonly IDuplexNetworkEndpointTransport _transport;
            private readonly IMethodCallObjectFactory _callFactory;
            private readonly object _localServer;
            private readonly CompactSerializerDictionary _dictionary;
            private IDeferred _outstandingCall;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ProxyBase(
                IComponentContext components, 
                CompactSerializer serializer, 
                IDuplexNetworkEndpointTransport transport, 
                IMethodCallObjectFactory callFactory,
                object localServer)
            {
                _components = components;
                _serializer = serializer;
                _transport = transport;
                _callFactory = callFactory;
                _localServer = localServer;
                _dictionary = new StaticCompactSerializerDictionary();

                // ReSharper disable once DoNotCallOverridableMethodsInConstructor
                OnRegisteringApiContracts(_dictionary);

                _outstandingCall = null;
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
                        var context = new CompactSerializationContext(_serializer, _dictionary, writer);
                        CompactRpcProtocol.WriteCall(call, context);
                        writer.Flush();

                        _transport.SendBytes(buffer.ToArray());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetOustandingCall(IDeferred deferred)
            {
                if (_outstandingCall != null)
                {
                    //TODO: add support for multiple concurrent promises per connection
                    throw new InvalidOperationException("Only one concurrent promise is supported per connection.");
                }

                //var deferred = new Deferred<T>();
                _outstandingCall = deferred;
                //return new Promise<T>(deferred);
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
                using (var buffer = new MemoryStream(bytes))
                {
                    using (var reader = new CompactBinaryReader(buffer))
                    {
                        var context = new CompactDeserializationContext(_serializer, _dictionary, reader, _components);
                        var call = CompactRpcProtocol.ReadCall(_callFactory, context);

                        using (DuplexNetworkApi.CurrentCall.UseClientProxy(this))
                        {
                            call.ExecuteOn(_localServer);
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ProxyConvention : ImplementationConvention
        {
            private readonly Type _remoteApiContract;
            private readonly Type _localApiContract;
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
                        .AllMethods(where: IsRemotableMethod).Implement(WriteRemoteContractMethod)
                        .AllEvents().ImplementAutomatic();
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsRemotableMethod(MethodInfo method)
            {
                return (
                    method.IsVoid() || 
                    typeof(IPromise).IsAssignableFrom(method.ReturnType));
            }

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
                var callLocal = writer.Local<IMethodCallObject>();
                
                callLocal.Assign(
                    _callFactoryField.Func<MethodInfo, IMethodCallObject>(x => x.NewMessageCallObject, 
                        writer.Const(writer.OwnerMethod.MethodDeclaration)));

                writer.ForEachArgument(
                    (arg, index) => {
                        callLocal.Void(x => x.SetParameterValue, writer.Const(index), arg.CastTo<object>());
                    });

                writer.This<ProxyBase<TT.TContract, TT.TContract2>>().Void(x => x.SendCall, callLocal);

                if (!writer.OwnerMethod.IsVoid && typeof(IPromise).IsAssignableFrom(writer.OwnerMethod.Signature.ReturnType))
                {
                    // we know that return type is Promise<T>
                    var resultType = writer.OwnerMethod.Signature.ReturnType.GetGenericArguments()[0];

                    using (TT.CreateScope<TT.TReturn, TT.TValue>(writer.OwnerMethod.Signature.ReturnType, resultType))
                    {
                        var deferredLocal = writer.Local<Deferred<TT.TValue>>(initialValue: writer.New<Deferred<TT.TValue>>());
                        writer.This<ProxyBase<TT.TContract, TT.TContract2>>().Void(x => x.SetOustandingCall, deferredLocal);

                        var promiseLocal = writer.Local<Promise<TT.TValue>>(initialValue: writer.New<Promise<TT.TValue>>(deferredLocal));

                        writer.Return(promiseLocal.CastTo<TT.TReturn>());

                        //writer.Return(writer.New<TT.TReturn>());
                    }
                }
            }
        }
    }
}
