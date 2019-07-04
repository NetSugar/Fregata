using System.Net.Sockets;
using Fregata.Buffers;

namespace Fregata.Sockets.Handlers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/3 13:36:04
    /// </summary>
    public abstract class BaseTcpConnectionHandler : ITcpConnectionHandler
    {
        public virtual void OnConnectionClosed(ITcpConnection tcpConnection, SocketError socketError)
        {
         
        }

        public virtual void OnMessageArrived(ITcpConnection tcpConnection, ReadResult message)
        {
        }
    }
}