using System.Collections.Generic;
using System.Net;

namespace Fregata.Sockets.Servers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/3 14:22:17
    /// </summary>
    public interface IServerSocket
    {
        long Id { get; }
        string Name { get; }
        IPEndPoint ListeningEndPoint { get; }
        int ClientCount { get; }
        List<ITcpConnection> Clients { get; }
    }
}