using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Endpoints.Core;
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

                _transport.BytesReceived += OnTransportBytesReceived;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract void OnRegisteringApiContracts(CompactSerializerDictionary dictionary);

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

            private void OnTransportBytesReceived(byte[] bytes)
            {
                using (var buffer = new MemoryStream(bytes))
                {
                    using (var reader = new CompactBinaryReader(buffer))
                    {
                        var context = new CompactDeserializationContext(_serializer, _dictionary, reader, _components);
                        var call = CompactRpcProtocol.ReadCall(_callFactory, context);
                        call.ExecuteOn(_localServer);
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
                        .VoidMethods().Implement(WriteRemoteContractMethod)
                        .AllEvents().ImplementAutomatic();
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteApiContractRegistrations(VoidMethodWriter writer, Argument<CompactSerializerDictionary> dictionary)
            {
                //throw new NotImplementedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteRemoteContractMethod(VoidMethodWriter writer)
            {
                //throw new NotImplementedException();
            }
        }
    }
}
