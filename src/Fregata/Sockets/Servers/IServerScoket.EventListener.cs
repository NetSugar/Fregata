namespace Fregata.Sockets.Servers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/3 13:58:41
    /// </summary>
    public interface IServerScoketEventListener
    {
        void OnServerStarted(IServerSocket server);

        void OnServerShutDown(IServerSocket server);
    }
}