using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests.Endpoints
{
    [TestFixture]
    public class DuplexTcpTransportTests : UnitTestBase
    {
        [Test]
        public void CanConnectToServer()
        {
            //-- arrange

            var connectedClientCount = 0;
            DuplexTcpTransport.Connection connection = null;

            var server = CreateServer(
                onClientConnected: c => {
                    connectedClientCount++;
                    connection = c;
                }
            );

            //-- act

            using (server)
            {
                using (CreateClient())
                {
                }
            }

            //-- assert

            connectedClientCount.ShouldBe(1);
            connection.ShouldNotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSendMessageToServer()
        {
            //-- arrange

            int messagesReceivedOnServer = 0;
            byte[] messageReceivedOnServer = null;

            var received = new ManualResetEvent(false);
            var timedOut = false;

            var server = CreateServer(
                onClientConnected: c => {
                    c.MessageReceived += (conn, bytes) => {
                        messagesReceivedOnServer++;
                        messageReceivedOnServer = bytes;
                        received.Set();
                    };
                }
            );

            //-- act

            using (server)
            {
                using (var client = CreateClient())
                {
                    client.SendMessage(BitConverter.GetBytes((int)0).Concat(Encoding.ASCII.GetBytes("HELLO")).ToArray());
                    timedOut = !received.WaitOne(10000);
                }
            }

            //-- assert

            timedOut.ShouldBe(false);

            messagesReceivedOnServer.ShouldBe(1);
            messageReceivedOnServer.ShouldNotBeNull();
            Encoding.ASCII.GetString(messageReceivedOnServer).ShouldBe("HELLO");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSendMessageToClient()
        {
            //-- arrange

            int messagesReceivedOnServer = 0;
            byte[] messageReceivedOnServer = null;

            int messagesReceivedOnClient = 0;
            byte[] messageReceivedOnClient = null;

            var received = new ManualResetEvent(false);
            var timedOut = false;

            var server = CreateServer(
                onClientConnected: c => {
                    c.MessageReceived += (conn, bytes) => {
                        messagesReceivedOnServer++;
                        messageReceivedOnServer = bytes;
                    };
                    c.SendMessage(Encoding.ASCII.GetBytes("HELLO"));
                }
            );

            var client = CreateClient(
                onMessageReceived: bytes => {
                    messagesReceivedOnClient++;
                    messageReceivedOnClient = bytes;
                    received.Set();
                }
            );

            //-- act

            using (server)
            {
                using (client)
                {
                    timedOut = !received.WaitOne(10000);
                }
            }

            //-- assert

            timedOut.ShouldBe(false);

            messagesReceivedOnServer.ShouldBe(0);
            messageReceivedOnServer.ShouldBeNull();
            
            messagesReceivedOnClient.ShouldBe(1);
            messageReceivedOnClient.ShouldNotBeNull();
            Encoding.ASCII.GetString(messageReceivedOnClient).ShouldBe("HELLO");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSendMessagesInMultipleThreads()
        {
            //-- arrange

            var senderCountOnServer = 10;
            var senderCountOnClient = 10;
            var messageCountPerSenderOnClient = 1000;
            var messageCountPerSenderOnServer = 1000;
            var messagesReceivedOnServer = new BlockingCollection<byte[]>();
            var messagesReceivedOnClient = new BlockingCollection<byte[]>();
            var senderTasks = new List<Task>();
            var connected = new ManualResetEvent(false);

            DuplexTcpTransport.Connection connection = null;
            long nextClientMessageId = 0;
            long nextServerMessageId = 0;

            var server = CreateServer(
                onClientConnected: c => {
                    connection = c;
                    connected.Set();
                }
            );

            var client = CreateClient(
                onMessageReceived: bytes => {
                    messagesReceivedOnClient.Add(bytes);
                }
            );

            Action<int> senderOnClient = (index) => {
                for (int i = 0 ; i < messageCountPerSenderOnClient ; i++)
                {
                    if (i < 2 || (i % 100) == 0)
                    {
                        Console.WriteLine("client sender #{0} sent {1} messages", index, i);
                    }

                    var messageId = Interlocked.Increment(ref nextClientMessageId);
                    client.SendMessage(CreateTestMessage(messageId, fromClient: true));
                    Thread.Sleep(0);
                }
            };

            Action<int> senderOnServer = (index) => {
                for (int i = 0; i < messageCountPerSenderOnServer; i++)
                {
                    if (i < 2 || (i % 100) == 0)
                    {
                        Console.WriteLine("server sender #{0} sent {1} messages", index, i);
                    }

                    var messageId = Interlocked.Increment(ref nextServerMessageId);
                    connection.SendMessage(CreateTestMessage(messageId, fromClient: false));
                    //Thread.Sleep(0);
                }
            };

            Action processingWitnessOnServer = () => {
                while (messagesReceivedOnServer.Count < senderCountOnClient * messageCountPerSenderOnClient)
                {
                    Thread.Sleep(100);
                }
            };

            Action processingWitnessOnClient = () => {
                while (messagesReceivedOnClient.Count < senderCountOnServer * messageCountPerSenderOnServer)
                {
                    Thread.Sleep(100);
                }
            };

            connected.WaitOne(10000).ShouldBeTrue("timed out waiting for client to connect");

            connection.MessageReceived += (c, bytes) => {
                messagesReceivedOnServer.Add(bytes);
            };

            bool sendersTimedOut;
            bool witnessesTimedOut;

            //-- act

            using (server)
            {
                using (client)
                {
                    for (int i = 0 ; i < senderCountOnClient; i++)
                    {
                        int senderIndex = i;
                        senderTasks.Add(Task.Factory.StartNew(() => senderOnClient(senderIndex), TaskCreationOptions.LongRunning));
                    }

                    for (int i = 0; i < senderCountOnServer; i++)
                    {
                        int senderIndex = i;
                        senderTasks.Add(Task.Factory.StartNew(() => senderOnServer(senderIndex), TaskCreationOptions.LongRunning));
                    }
                    
                    sendersTimedOut = !Task.WaitAll(senderTasks.ToArray(), 20000);
                    witnessesTimedOut = !Task.WaitAll(
                        new[] {
                            Task.Factory.StartNew(processingWitnessOnServer, TaskCreationOptions.LongRunning),
                            Task.Factory.StartNew(processingWitnessOnClient, TaskCreationOptions.LongRunning)
                        },
                        20000
                    );
                }
            }

            //-- assert

            sendersTimedOut.ShouldBeFalse("timed out waiting for senders to finish");
            witnessesTimedOut.ShouldBeFalse("timed out waiting for witness that all messages were read");

            messagesReceivedOnServer.Count.ShouldBe(senderCountOnClient * messageCountPerSenderOnClient);
            messagesReceivedOnClient.Count.ShouldBe(senderCountOnServer * messageCountPerSenderOnServer);

            var messageIdsReceivedOnServer = new HashSet<long>();
            var messageIdsReceivedOnClient = new HashSet<long>();
            byte[] message;

            while (messagesReceivedOnServer.TryTake(out message))
            {
                messageIdsReceivedOnServer.Add(ValidateTestMessage(message, fromClient: true));
            }

            while (messagesReceivedOnClient.TryTake(out message))
            {
                messageIdsReceivedOnClient.Add(ValidateTestMessage(message, fromClient: false));
            }

            messageIdsReceivedOnServer.Count.ShouldBe(senderCountOnClient * messageCountPerSenderOnClient);
            messageIdsReceivedOnClient.Count.ShouldBe(senderCountOnServer * messageCountPerSenderOnServer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanFastCancelReceiveFromSocket()
        {
            //-- arrange

            var cancellation1 = new CancellationTokenSource();
            var cancellation2 = new CancellationTokenSource();
            var cancellation3 = CancellationTokenSource.CreateLinkedTokenSource(cancellation1.Token, cancellation2.Token);
            Func<Task<TimeSpan>> taskDelegate = async () => {
                var clock = Stopwatch.StartNew();
                using (var server = CreateServer())
                {
                    using (var client = new TcpClient("localhost", 9595))
                    {
                        // the workaround trick is to close TcpClient upon cancellation
                        cancellation3.Token.Register(() => client.Close()); 
                        
                        var buffer = new byte[8];
                        try
                        {
                            // passing CancellationToken is of no use, as it is ignored by ReadAsync implementation
                            var bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
                return clock.Elapsed;
            };

            //-- act

            var cancellationRequested0 = cancellation3.IsCancellationRequested;
            var taskToCancel = taskDelegate();

            Thread.Sleep(250);

            cancellation1.Cancel();

            var cancellationRequested1 = cancellation3.IsCancellationRequested;

            var taskWasCancelled = taskToCancel.Wait(1000);
            var taskDuration = taskToCancel.Result;

            //-- assert

            cancellationRequested0.ShouldBe(false);
            cancellationRequested1.ShouldBe(true);
            taskWasCancelled.ShouldBe(true);
            taskDuration.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(250));
            taskDuration.ShouldBeLessThan(TimeSpan.FromMilliseconds(5000));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DuplexTcpTransport.Server CreateServer(
            Action<DuplexTcpTransport.Connection> onClientConnected = null,
            Action<DuplexTcpTransport.Connection, ConnectionCloseReason> onClientDisconnected = null)
        {
            var server = new DuplexTcpTransport.Server(
                Framework.Logger<DuplexTcpTransport.Logger>(),
                ip: null,
                port: 9595,
                onClientConnected: onClientConnected,
                onClientDisconnected: onClientDisconnected);

            server.Start();

            return server;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private DuplexTcpTransport.Client CreateClient(
            Action<byte[]> onMessageReceived = null,
            Action<ConnectionCloseReason> onDisconnected = null)
        {
            var client = new DuplexTcpTransport.Client(
                Framework.Logger<DuplexTcpTransport.Logger>(),
                serverHost: "localhost",
                serverPort: 9595,
                onMessageReceived: onMessageReceived,
                onDisconnected: onDisconnected);

            client.Start();

            return client;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private unsafe byte[] CreateTestMessage(long messageId, bool fromClient)
        {
            var repeatCount = 1 + (fromClient ? (messageId % 17) : (messageId % 257));
            var message = new byte[sizeof(long) * repeatCount];

            fixed (byte* pFirst = message)
            {
                int i;
                byte* pNext;

                for (i = 0, pNext = pFirst ; i < repeatCount ; i++, pNext += sizeof(long))
                {
                    *((long*)pNext) = messageId;
                }
            }

            return message;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private unsafe long ValidateTestMessage(byte[] message, bool fromClient)
        {
            fixed (byte* pFirst = message)
            {
                long messageId = *((long*)pFirst);
                var repeatCount = 1 + (fromClient ? (messageId % 17) : (messageId % 257));

                ((long)message.Length).ShouldBe(sizeof(long) * repeatCount, "Length mismatch for message id " + messageId);

                int i;
                byte* pNext;

                for (i = 0, pNext = pFirst; i < repeatCount; i++, pNext += sizeof(long))
                {
                    var value = *((long*)pNext);
                    value.ShouldBe(messageId, "Value at index " + i + " mismatch for message id " + messageId);
                }

                return messageId;
            }
        }
    }
}
