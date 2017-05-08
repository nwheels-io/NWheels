using NWheels.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    /// <summary>
    /// Configuration of the messaging platform
    /// </summary>
    [ConfigSection]
    public interface IMessagingPlatformConfiguration
    {
        IHttpEndpointConfig NewHttpEndpointConfig();
        IHttpsConfig NewHttpsConfig();
        IHttpStaticFolderConfig NewHttpStaticFolderConfig();

        /// <summary>
        /// Endpoints by name
        /// </summary>
        IDictionary<string, IEndpointConfig> Endpoints { get; }
    }
}
