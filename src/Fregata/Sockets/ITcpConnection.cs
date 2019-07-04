using System.Net;

namespace Fregata.Sockets
{
    public interface ITcpConnection
    {
        long Id { get; }
        string Name { get; }
        bool IsConnected { get; }
        EndPoint LocalEndPoint { get; }
        EndPoint RemotingEndPoint { get; }

        void Send(byte[] message);

        void Close();
    }
}