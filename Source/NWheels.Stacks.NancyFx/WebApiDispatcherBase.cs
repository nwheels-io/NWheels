using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using DispatchCallback = System.Func<Nancy.INancyModule, object, object>;

namespace NWheels.Stacks.NancyFx
{
    public abstract class WebApiDispatcherBase
    {
        private readonly Dictionary<string, DispatchCallback> _callbackByOperationName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected WebApiDispatcherBase()
        {
            _callbackByOperationName = new Dictionary<string, DispatchCallback>(StringComparer.InvariantCultureIgnoreCase);
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            RegisterOperations();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object DispatchOperation(string name, INancyModule webModule, object service)
        {
            DispatchCallback callback;

            if ( !_callbackByOperationName.TryGetValue(name, out callback) )
            {
                return HttpStatusCode.NotFound;
            }

            return callback(webModule, service);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void RegisterOperations();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterOperation(string name, DispatchCallback callback)
        {
            _callbackByOperationName.Add(name, callback);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly BindingConfig _s_modelBindingConfig = new BindingConfig {
            BodyOnly = true
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static BindingConfig ModelBindingConfig
        {
            get { return _s_modelBindingConfig; }
        }
    }
}
