using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Client;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var framework = ClientSideFramework.CreateWithDefaultConfiguration();
            var clientFactory = framework.Components.Resolve<DuplexTcpClientFactory>();
            var client = new ChatClient();

            Console.WriteLine("Now connecting to chat server.");
            Console.WriteLine("HELP > while in chat, type your message and hit ENTER to send.");
            Console.WriteLine("HELP > to leave, type Q and hit ENTER.");

            client.Server = clientFactory.CreateServerProxy<IChatServerApi, IChatClientApi>(
                new ChatClient(),
                serverHostname: "localhost",
                serverPort: 9797,
                serverPingInterval: TimeSpan.FromSeconds(1));

            client.Server.Hello(myName: "PID#" + Process.GetCurrentProcess().Id);

            while (true)
            {
                var text = Console.ReadLine();
                
                if (text == null || text.Trim().EqualsIgnoreCase("Q"))
                {
                    client.Server.GoodBye();
                    break;
                }
            }

            Console.WriteLine("Shutting down.");
        }
    }
}
