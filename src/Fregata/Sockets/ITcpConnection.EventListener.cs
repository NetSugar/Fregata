using System.Net;
using System.Net.Sockets;

namespace Fregata.Sockets
{
    public interface ITcpConnectionEventListener
    {
        void OnConnectionAccepted(ITcpConnection connection);

        void OnConnectionEstablished(ITcpConnection connection);

        void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError);

        void OnConnectionClosed(ITcpConnection connection, SocketError socketError);
    }
}