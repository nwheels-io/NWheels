using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public static class MessageProtocolInfo
    {
        /// <summary>
        /// This property allows calling extension methods on MessageProtocols.IExtension interface.
        /// </summary>
        public static IExtension Select => null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The sole purpose of this interface is allow writing extension methods on it
        /// </summary>
        public interface IExtension
        {
        }
    }
}
