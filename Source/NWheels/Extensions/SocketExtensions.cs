using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class SocketExtensions
    {
        public static async Task<bool> ReadAsync(this Socket s, byte[] buffer, int offset, int count, TimeSpan timeout)
        {
            // Reusable SocketAsyncEventArgs and awaitable wrapper 
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, count);
            var awaitable = new SocketAwaitable(args);
            // Do processing, continually receiving from the socket 
            while (true)
            {
                await s.ReceiveAsync(awaitable);
                int bytesRead = args.BytesTransferred;
                if (bytesRead <= 0)
                {
                    return false;
                }
                if (bytesRead >= count)
                {
                    return true;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveAsync(awaitable.EventArgs))
            {
                awaitable.WasCompleted = true;
            }
            return awaitable;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static SocketAwaitable SendAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.SendAsync(awaitable.EventArgs))
            {
                awaitable.WasCompleted = true;
            }
            return awaitable;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public sealed class SocketAwaitable : INotifyCompletion
        {
            private static readonly Action _s_sentinel = () => { };

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            internal bool WasCompleted;
            internal Action Continuation;
            internal SocketAsyncEventArgs EventArgs;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public SocketAwaitable(SocketAsyncEventArgs eventArgs)
            {
                if (eventArgs == null)
                {
                    throw new ArgumentNullException("eventArgs");
                }

                EventArgs = eventArgs;

                eventArgs.Completed += (sender, args) =>
                {
                    var prev = Continuation ?? Interlocked.CompareExchange(ref Continuation, _s_sentinel, null);
                    if (prev != null)
                    {
                        prev();
                    }
                };
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public SocketAwaitable GetAwaiter()
            {
                return this;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsCompleted
            {
                get { return WasCompleted; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void OnCompleted(Action continuation)
            {
                if (Continuation == _s_sentinel || Interlocked.CompareExchange(ref Continuation, continuation, null) == _s_sentinel)
                {
                    Task.Run(continuation);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void GetResult()
            {
                if (EventArgs.SocketError != SocketError.Success)
                {
                    throw new SocketException((int)EventArgs.SocketError);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            internal void Reset()
            {
                WasCompleted = false;
                Continuation = null;
            }
        }
    }
}
