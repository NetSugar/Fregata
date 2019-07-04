using Fregata.Sockets.Servers;

namespace Fregata.Sockets.Handlers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/4 10:52:45
    /// </summary>
    public interface ITcpConnectionServerHandler : ITcpConnectionHandler
    {
        IServerSocket ServerSocket { get; set; }
    }
}