using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
            if (args.Contains("--poc"))
            {
                PocMain(args);
                return;
            }

            NWheels.Stacks.Nlog.NLogBasedPlainLog.Instance.ConfigureConsoleOutput(NLog.LogLevel.Debug);
            var framework = ClientSideFramework.CreateWithDefaultConfiguration(
                new NWheels.Stacks.Nlog.ModuleLoader()
            );

            var apiFactory = framework.Components.Resolve<DuplexTcpTransport.ApiFactory>();
            var client = new ChatClient();
            apiFactory.CreateApiClient<IChatServiceApi, IChatClientApi>(
                client,
                serverHost: "localhost",
                serverPort: 9797,
                clientToServerHeartbeatInterval: TimeSpan.FromSeconds(1));

            Console.WriteLine("Now connecting to chat server.");
            Console.WriteLine("HELP > while in chat, type your message and hit ENTER to send.");
            Console.WriteLine("HELP > to leave, type Q and hit ENTER.");

            client.Server.Hello(myName: "PID#" + Process.GetCurrentProcess().Id);

            while (true)
            {
                var text = Console.ReadLine();
                
                if (text == null || text.Trim().EqualsIgnoreCase("Q"))
                {
                    client.Server.GoodBye();
                    break;
                }

                client.Server.SayToOthers(text);
            }

            Console.WriteLine("Shutting down.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void PocMain(string[] args)
        {
            Console.WriteLine("****** Starting PoC.");

            NWheels.Stacks.Nlog.NLogBasedPlainLog.Instance.ConfigureConsoleOutput(NLog.LogLevel.Debug);
            ClientSideFramework framework = ClientSideFramework.CreateWithDefaultConfiguration(new NWheels.Stacks.Nlog.ModuleLoader());

            var serverArgIndex = Array.IndexOf(args, "--server");
            if (serverArgIndex < 0 || serverArgIndex >= args.Length - 1)
            {
                throw new ArgumentException("server must be specified in the format --server host:port");
            }

            var serverAddress = args[serverArgIndex + 1];
            var serverUri = new Uri("tcp://" + serverAddress);

            DuplexTcpTransport.Server server = null;
            DuplexTcpTransport.Client client = null;
            DuplexTcpTransport.Connection serverSession = null;

            if (args.Contains("--run-server"))
            {
                var serverIp = (Uri.CheckHostName(serverUri.Host) == UriHostNameType.IPv4 ? serverUri.Host : null);
                server = new DuplexTcpTransport.Server(
                    framework.Logger<DuplexTcpTransport.Logger>(),
                    serverIp, 
                    serverUri.Port, 
                    onClientConnected: (session) => {
                        serverSession = session;
                        session.MessageReceived += (session2, messageBytes) => {
                            Console.WriteLine("CLIENT -> SERVER : {0}", Encoding.UTF8.GetString(messageBytes));
                        };
                    });
                server.Start();
            }

            if (args.Contains("--run-client"))
            {
                client = new DuplexTcpTransport.Client(
                    framework.Logger<DuplexTcpTransport.Logger>(),
                    serverUri.Host, 
                    serverUri.Port, 
                    onMessageReceived: (messageBytes) => {
                        Console.WriteLine("SERVER -> CLIENT : {0}", Encoding.UTF8.GetString(messageBytes));
                    });
                client.Start();
                Task.Factory.StartNew(() => TcpPoc.RunScenario2(client, serverSession));
            }

            Console.WriteLine("****** PoC is running. Hit ENTER to quit . . .");
            Console.WriteLine();

            Console.ReadLine();

            if (client != null)
            {
                client.Dispose();
            }

            if (server != null)
            {
                server.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("****** PoC shut down completed.");
        }
    }
}
