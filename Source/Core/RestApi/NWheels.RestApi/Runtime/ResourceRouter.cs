using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public class ResourceRouter : IResourceRouter
    {
        private readonly Dictionary<string, ProtocolBindingMap> _bindingMapByProtocol;
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public ResourceRouter(IEnumerable<ICompiledResourceCatalog> allCatalogs)
        {
            _bindingMapByProtocol = new Dictionary<string, ProtocolBindingMap>();

            foreach (var catalog in allCatalogs)
            {
                catalog.PopulateRouter(_bindingMapByProtocol);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetResourceBinding(string uriPath, string protocol, out IResourceBinding binding)
        {
            if (_bindingMapByProtocol.TryGetValue(protocol, out ProtocolBindingMap protocolBindings))
            {
                return protocolBindings.TryGetValue(uriPath, out binding);
            }

            binding = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetResourceBinding<TMessageContext>(string uriPath, string protocol, out IResourceBinding<TMessageContext> binding)
        {
            if (_bindingMapByProtocol.TryGetValue(protocol, out ProtocolBindingMap protocolBindings))
            {
                if (protocolBindings.TryGetValue(uriPath, out IResourceBinding nonTypedBinding))
                {
                    binding = nonTypedBinding as IResourceBinding<TMessageContext>;
                    return (binding != null);
                }
            }

            binding = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

//        public IEnumerable<IResourceHandler> GetAllResourceHandlers()
//        {
//            return _handlerByUriPath.Values;
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//
//        public bool TryGetResourceHandler(string uriPath, out IResourceHandler handler)
//        {
//            return _handlerByUriPath.TryGetValue(uriPath, out handler);
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//
//        public IEnumerable<IResourceBinding> GetResourceBindings(IResourceHandler handler)
//        {
//            return _bindingByProtocolByUriPath
//                .Values
//                .Select(bindings => bindings.ContainsKey(handler.FullUrlPath) ? bindings[handler.FullUrlPath] : null)
//                .Where(binding => binding != null);
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//
//        public bool TryGetResourceBinding(IResourceHandler handler, string protocol, out IResourceBinding binding)
//        {
//            if (_bindingByProtocolByUriPath.TryGetValue(protocol, out IReadOnlyDictionary<string, IResourceBinding> protocolBindings))
//            {
//                return protocolBindings.TryGetValue(handler.FullUrlPath, out binding);
//            }
//
//            binding = null;
//            return false;
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//
//        public bool TryGetResourceBinding<TMessageContext>(string uriPath, string protocol, out IResourceBinding<TMessageContext> binding)
//        {
//            if (_bindingByProtocolByUriPath.TryGetValue(protocol, out IReadOnlyDictionary<string, IResourceBinding> protocolBindings))
//            {
//                if (protocolBindings.TryGetValue(uriPath, out IResourceBinding nonTypedBinding))
//                {
//                    binding = nonTypedBinding as IResourceBinding<TMessageContext>;
//                    return (binding != null);
//                }
//            }
//
//            binding = null;
//            return false;
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//        
//        private static Dictionary<string, IReadOnlyDictionary<string, IResourceBinding>> BuildBindingByProtocolByUriPathMap(IEnumerable<IResourceBindingCatalog> bindingCatalogs)
//        {
//            var outerMap = new Dictionary<string, IReadOnlyDictionary<string, IResourceBinding>>();
//
//            foreach (var catalog in bindingCatalogs)
//            {
//                if (!outerMap.TryGetValue(catalog.Protocol, out IReadOnlyDictionary<string, IResourceBinding> innerMap))
//                {
//                    innerMap = new Dictionary<string, IResourceBinding>(1024, StringComparer.InvariantCultureIgnoreCase);
//                    outerMap.Add(catalog.Protocol, innerMap);
//                }
//
//                catalog.PopulateBindings((IDictionary<string, IResourceBinding>)innerMap);
//            }
//            
//            return outerMap;
//        }
//
//        //-----------------------------------------------------------------------------------------------------------------------------------------------------
//        
//        private static Dictionary<string, IResourceHandler> BuildHandlerByUriPathMap(IEnumerable<IResourceCatalog> resourceCatalogs)
//        {
//            var handlerMap = new Dictionary<string, IResourceHandler>(1024, StringComparer.InvariantCultureIgnoreCase);
//
//            foreach (var catalog in resourceCatalogs)
//            {
//                catalog.PopulateRouter(handlerMap);
//            }
//            
//            return handlerMap;
//        }
    }
}