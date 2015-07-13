using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace NWheels.Stacks.AspNet
{
    public class DynamicControllerTypeResolver : IHttpControllerTypeResolver
    {
        private readonly ControllerInitializer _initializer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicControllerTypeResolver(ControllerInitializer initializer)
        {
            _initializer = initializer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            var types = _initializer.ControllerTypes.ToArray();
            return types;
        }
    }
}
