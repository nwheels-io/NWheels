using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpOperationAttribute : Attribute
    {
        public HttpOperationAttribute()
        {
            Verbs = HttpOperationVerbs.Get;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Route { get; set; }
        public HttpOperationVerbs Verbs { get; set; }
    }
}
