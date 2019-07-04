using Fregata.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Fregata.Sockets
{
    internal static class SocketUtil
    {
        public static IPAddress GetLocalIPV4()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
        }

        public static IPAddress GetLocalIPV6()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetworkV6);
        }

        public static Socket CreateSocket(EndPoint serverEndPoint, int sendBufferSize, int receiveBufferSize)
        {
            if (serverEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return CreateSocket4IPV6(sendBufferSize, receiveBufferSize);
            }
            else if (serverEndPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                return CreateSocket4IPV4(sendBufferSize, receiveBufferSize);
            }
            else
            {
                throw new NotSupportedException("listening endpoint not suppoted AddressFamily:");
            }
        }

        public static Socket CreateSocket4IPV4(int sendBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                SendBufferSize = sendBufferSize,
                ReceiveBufferSize = receiveBufferSize
            };
            return socket;
        }

        public static Socket CreateSocket4IPV6(int sendBufferSize, int receiveBufferSize)
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                Blocking = false,
                SendBufferSize = sendBufferSize,
                ReceiveBufferSize = receiveBufferSize
            };
            return socket;
        }

        public static ValueTask<int> ReceiveAsync(this Socket socket, Memory<byte> buffer)
        {
            return socket.ReceiveAsync(buffer, SocketFlags.None);
        }

        public static void ShutdownSocket(Socket socket)
        {
            if (socket == null) return;

            ExceptionUtil.Eat(() => socket.Shutdown(SocketShutdown.Both));
            ExceptionUtil.Eat(() => socket.Close(10000));
            ExceptionUtil.Eat(() => socket.Dispose());
        }
    }
}