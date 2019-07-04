using Fregata.Buffers;
using System.Net.Sockets;

namespace Fregata.Sockets.Handlers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/1 17:33:24
    /// </summary>
    public interface ITcpConnectionHandler
    {
        void OnMessageArrived(ITcpConnection tcpConnection, ReadResult message);

        void OnConnectionClosed(ITcpConnection tcpConnection, SocketError socketError);
    }
}