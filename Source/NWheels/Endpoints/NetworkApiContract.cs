using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public static class NetworkApiContract
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class ConnectCommandAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method)]
        public class DisconnectCommandAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
        public class ConnectedEventAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
        public class ConnectDeclinedEventAttribute : Attribute
        {
        }
    }
}
