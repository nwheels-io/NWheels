using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NWheels.Stacks.Network
{
    public class TcpSocketsUtils
    {
        //----------- Send Section ---------------
        public delegate void OnSendDlgt();
        private class SendState
        {
            public Socket Socket;
            public OnSendDlgt OnSend;
            public OnExcpDlgt OnException;
            public byte[] Buffer;
            public int BytesSentSoFar;
        }

        // Send a buffer on the given socket.
        // The onSocket delegate will be executed once the send action has ended
        public static void Send(Socket socket, string s, OnSendDlgt onSend, OnExcpDlgt onExcp)
        {
            byte[] bufferToSend = Encoding.UTF8.GetBytes(s);
            Send(socket, bufferToSend, onSend, onExcp);
        }

        private static void ConvertIntToArray(int Val, out byte[] OutArray)
        {
            byte[] maxNum = new byte[MaxNumOfBytesInLengthField];
            int arrayLen = 0;

            do
            {
                maxNum[arrayLen++] = (byte)(Val & 0x7F);
                Val >>= 7;
            } while (Val != 0);

            OutArray = new byte[arrayLen];
            int i;
            for (i = 0; i < arrayLen; i++)
            {
                OutArray[i] = (byte)(maxNum[arrayLen - i - 1] | (byte)((i == (arrayLen - 1)) ? 0x0 : 0x80));
            }
        }

        // Send a byte array on the given socket.
        // The onSocket delegate will be executed once the send action has ended
        public static void Send(Socket socket, byte[] bufferToSend, OnSendDlgt onSend, OnExcpDlgt onExcp)
        {
            try
            {
                byte[] lenBuf;

                ConvertIntToArray(bufferToSend.Length, out lenBuf);

                SendState state = new SendState();
                state.Socket = socket;
                state.OnSend = onSend;
                state.OnException = onExcp;
                state.Buffer = new byte[lenBuf.Length + bufferToSend.Length];
                state.BytesSentSoFar = 0;
                Buffer.BlockCopy(lenBuf, 0, state.Buffer, 0, lenBuf.Length);
                Buffer.BlockCopy(bufferToSend, 0, state.Buffer, lenBuf.Length, bufferToSend.Length);

                //Begin sending the data to the remote device.
                state.Socket.BeginSend(
                    state.Buffer,
                    0,
                    state.Buffer.Length,
                    0,
                    SendCallback,
                    state);
            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                onExcp(e);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            SendState state = (SendState)ar.AsyncState;
            try
            {
                if (state.Socket.Connected == false)
                {
                    return;
                }

                // Complete sending the data to the remote device.
                int bytesSent = state.Socket.EndSend(ar);
                state.BytesSentSoFar += bytesSent;
                if (state.BytesSentSoFar < state.Buffer.Length)
                {
                    //keep sending
                    state.Socket.BeginSend(
                        state.Buffer,
                        state.BytesSentSoFar,
                        state.Buffer.Length - state.BytesSentSoFar,
                        0,
                        SendCallback,
                        state);

                }
                else
                {
                    state.OnSend();
                }

                // Signal that all bytes have been sent.
            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                //LogUtils.LogError(LogUtils.DefaultLogNamespace, "{0}", e);
                state.OnException(e);
            }
        }

        //----------- Recv Section ---------------

        /// <summary>
        /// This delegate will be called upon a new buffer arrived.
        /// NOTE: you MUSTN'T do any heavy action in your called method.
        /// </summary>
        /// <param name="buf"></param>
        public delegate bool OnRecvDlgt(byte[] buf); //return false - indicates stop recv loop
        public delegate void OnExcpDlgt(Exception e);

        private const int MaxNumOfBytesInLengthField = 5;

        private class RecvState
        {
            public Socket Socket;
            public OnRecvDlgt OnRecv;
            public OnExcpDlgt OnExcp;
            public byte[] DateBuffer;
            public byte[] PrevLeftoverBuf;
            public bool RecvLoop;
        }

        public static void Recv(Socket socket, OnRecvDlgt onRecv, OnExcpDlgt onExcp, int reciveBufferSize, bool recvLoop)
        {
            try
            {
                // Create the state object.
                RecvState state = new RecvState();
                state.Socket = socket;
                state.OnRecv = onRecv;
                state.OnExcp = onExcp;
                state.RecvLoop = recvLoop;
                state.DateBuffer = (recvLoop ? new byte[reciveBufferSize] : new byte[1]);
                StartRecvPacket(state);
            }
            catch (Exception e)
            {
                if (!(e is ObjectDisposedException))
                {
                    // Todo: report (log) the exception 
                    //LogUtils.LogError(LogUtils.DefaultLogNamespace, "{0}", e);
                }
            }
        }

        //
        // NOTE NOTE NOTE:
        //
        // BeginReceive() receives a callback which will be called upon completion of the action.
        // The callback might be called from a different thread - OR -
        // it can be called in this thread from within the BeginReceive() function.
        // Keep that in mind: You should not change or rely on any state after you call this function,
        // otherwise you might get some serious bugs in your system.
        //
        private static void StartRecvPacket(RecvState state)
        {
            // Begin receiving the data from the remote device, read buf len
            if (state.Socket.Connected)
            {
                state.Socket.BeginReceive(
                    state.DateBuffer,
                    0, //offset
                    state.DateBuffer.Length, //len of len (start with 1 byte, if needed we'll read more later)
                    0, //flags
                    new AsyncCallback(RecvCallbackFromDevice),
                    state);
            }
        }

        private static void RecvCallbackFromDevice(IAsyncResult ar)
        {
            RecvState state = (RecvState)ar.AsyncState;
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.

                // can get the disconnected socket status
                if (state.Socket.Connected == false)
                {
                    return;
                }
                // Read data from the remote device.
                int bytesRead = state.Socket.EndReceive(ar);
                ParseBuffer(state, bytesRead);
            }
            catch (ObjectDisposedException)
            {
                // DisposedException is received if the current host initiated
                // a disconnection. In this case - do nothing.
            }
            catch (Exception e)
            {
                state.OnExcp(e);
            }
        }

        private static void ParseBuffer(RecvState state, int dataLength)
        {
            if (dataLength == 0)
            {
                return;
            }
            byte[] workingBuf;
            if (state.PrevLeftoverBuf != null)
            {
                workingBuf = new byte[dataLength + state.PrevLeftoverBuf.Length];
                Buffer.BlockCopy(state.PrevLeftoverBuf, 0, workingBuf, 0, state.PrevLeftoverBuf.Length);
                Buffer.BlockCopy(state.DateBuffer, 0, workingBuf, state.PrevLeftoverBuf.Length, dataLength);
                //Array.Copy(state.m_PrevLeftoverBuf, workingBuf, state.m_PrevLeftoverBuf.Length);
                //Array.Copy(state.m_buf, 0, workingBuf, state.m_PrevLeftoverBuf.Length, dataLength);
                state.PrevLeftoverBuf = null;
                dataLength = workingBuf.Length;
            }
            else
            {
                workingBuf = state.DateBuffer;
            }

            for (int i = 0; i < dataLength; )
            {
                /********************************************************************/
                // Read packet length
                /********************************************************************/
                int packetLenBytes = 1;
                int expectedTotalPacketLen = workingBuf[i] & 0x7F;
                while ((workingBuf[i] & 0x80) == 0x80)
                {
                    i++;
                    if (i < dataLength)
                    {
                        expectedTotalPacketLen <<= 7;
                        expectedTotalPacketLen += workingBuf[i] & 0x7F;
                        packetLenBytes++;
                    }
                    else
                    {
                        state.PrevLeftoverBuf = new byte[packetLenBytes];
                        Buffer.BlockCopy(workingBuf, i - packetLenBytes, state.PrevLeftoverBuf, 0, packetLenBytes);
                        //Array.Copy(workingBuf, i - packetLenBytes, state.m_PrevLeftoverBuf, 0, packetLenBytes);
                        StartRecvPacket(state);
                        return;
                    }
                }
                /********************************************************************/
                // End Read packet length
                /********************************************************************/
                i++;
                int packetBytesRecived = Math.Min(expectedTotalPacketLen, dataLength - i);
                if (packetBytesRecived < expectedTotalPacketLen)
                {
                    state.PrevLeftoverBuf = new byte[packetBytesRecived + packetLenBytes];
                    Array.Copy(workingBuf, i - packetLenBytes, state.PrevLeftoverBuf, 0, packetBytesRecived + packetLenBytes);
                    if (!state.RecvLoop)
                    {
                        state.DateBuffer = new byte[expectedTotalPacketLen - packetBytesRecived];
                    }

                    StartRecvPacket(state);
                    return;
                }

                byte[] data = new byte[packetBytesRecived];
                Array.Copy(workingBuf, i, data, 0, packetBytesRecived);
                state.OnRecv(data);
                i += packetBytesRecived;
            }
            if (state.RecvLoop)
            {
                StartRecvPacket(state);
            }
        }

        //----------- Connect Section ---------------
        public static Socket Connect(string remoteAddr, int port)
        {
            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(remoteAddr);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ep = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket socket = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Connect to the remote endpoint.
            socket.Connect(ep);
            return socket;
        }
    }
}
