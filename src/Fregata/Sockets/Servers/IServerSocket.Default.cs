using Fregata.Buffers;
using Fregata.Options;
using Fregata.Sockets.Handlers;
using Fregata.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Fregata.Sockets.Servers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/27 18:44:55
    /// </summary>
    public class ServerSocket : IServerSocket
    {
        private readonly IBufferPool _receivedBufferPool;
        private readonly IBufferPool _sendBufferPool;
        private readonly SocketAsyncEventArgs _acceptSocketArgs;
        private readonly ConcurrentDictionary<long, ITcpConnection> _connectionDict;
        private readonly IList<ITcpConnectionEventListener> _connectionEventListeners;
        private readonly IList<IServerScoketEventListener> _serverScoketEventListeners;
        private readonly ITcpConnectionServerHandler _tcpConnectionServerHandler;

        public ServerSocket(string name, IPEndPoint listeningEndPoint, FregataOptions fregataOptions, IBufferPool receivedBufferPool, IBufferPool sendBufferPool, ITcpConnectionServerHandler tcpConnectionServerHandler)
        {
            if (string.IsNullOrWhiteSpace(name)) ThrowHelper.ThrowServerNameNotBeNullErrorException();
            if (listeningEndPoint == null) ThrowHelper.ThrowListenEndPointNotBeNullErrorException();
            Id = SnowflakeId.Default().NextId();
            Name = name;
            ListeningEndPoint = listeningEndPoint;
            Setting = fregataOptions;
            _receivedBufferPool = receivedBufferPool ?? throw new ArgumentNullException("receivedBufferPool");
            _sendBufferPool = sendBufferPool ?? throw new ArgumentNullException("sendBufferPool");
            _tcpConnectionServerHandler = tcpConnectionServerHandler ?? throw new ArgumentNullException("tcpConnectionServerHandler");
            tcpConnectionServerHandler.ServerSocket = this;
            Socket = SocketUtil.CreateSocket(listeningEndPoint, fregataOptions.SendMaxSize, fregataOptions.ReceiveMaxSize);
            _connectionDict = new ConcurrentDictionary<long, ITcpConnection>();
            _connectionEventListeners = new List<ITcpConnectionEventListener>();
            _serverScoketEventListeners = new List<IServerScoketEventListener>();
            _acceptSocketArgs = new SocketAsyncEventArgs();
            _acceptSocketArgs.Completed += AcceptCompleted;
            _connectionEventListeners.Add(new ServerTcpConnectionEventListener(this));
        }

        public long Id { get; }
        public string Name { get; }
        public IPEndPoint ListeningEndPoint { get; }
        public FregataOptions Setting { get; }
        public Socket Socket { get; private set; }
        public int ClientCount => _connectionDict.Count;
        public List<ITcpConnection> Clients => _connectionDict.Values.ToList();

        public void RegisterConnectionEventListener(ITcpConnectionEventListener listener)
        {
            _connectionEventListeners.Add(listener);
        }

        public void Start()
        {
            Log<ServerSocket>.Info(string.Format("Socket server is starting, name: {0}, listening on listeningEndPoint: {1}.", Name, ListeningEndPoint));

            try
            {
                Socket.Bind(ListeningEndPoint);
                Socket.Listen(Setting.ServerMaxPendingConnections);
            }
            catch (Exception ex)
            {
                Log<ServerSocket>.Error(ex, string.Format("Socket server start failed, name: {0}, listeningEndPoint: {1}.", Name, ListeningEndPoint));
                SocketUtil.ShutdownSocket(Socket);
                throw;
            }
            ServerStateChanged(ServerSocketStateChangedState.Started);
            StartAccepting();
        }

        private void StartAccepting()
        {
            try
            {
                var firedAsync = Socket.AcceptAsync(_acceptSocketArgs);
                if (!firedAsync)
                {
                    ProcessAccept(_acceptSocketArgs);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ObjectDisposedException))
                {
                    Log<ServerSocket>.Error(ex, string.Format("Socket server accept has exception, name: {0}, listeningEndPoint: {1}.", Name, ListeningEndPoint));
                }
                Task.Factory.StartNew(() => StartAccepting());
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var acceptSocket = e.AcceptSocket;
                    e.AcceptSocket = null;
                    OnSocketAccepted(acceptSocket, e.UserToken);
                }
                else
                {
                    SocketUtil.ShutdownSocket(e.AcceptSocket);
                    e.AcceptSocket = null;
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Log<ServerSocket>.Error(ex, string.Format("Socket server process accept has exception, name: {0}, listeningEndPoint: {1}.", Name, ListeningEndPoint));
            }
            finally
            {
                StartAccepting();
            }
        }

        private void OnSocketAccepted(Socket socket, object userToken)
        {
            Task.Run(() =>
            {
                try
                {
                    var connection = new TcpConnection(Name, socket, Setting, _receivedBufferPool, _sendBufferPool, _tcpConnectionServerHandler);
                    connection.RegisterConnectionEventListener(_connectionEventListeners);
                    if (_connectionDict.TryAdd(connection.Id, connection))
                    {
                        Log<ServerSocket>.Info(string.Format("Socket server new client accepted, name: {0}, listeningEndPoint: {1}, remoteEndPoint: {2}", Name, ListeningEndPoint, socket.RemoteEndPoint));
                        connection.Accepted();
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Log<ServerSocket>.Error(ex, string.Format("Socket server accept new client has unknown exception, name: {0}, listeningEndPoint: {1}", Name, ListeningEndPoint));
                }
            });
        }

        private void RemoveConnection(long id)
        {
            _connectionDict.Remove(id, out _);
        }

        public void Shutdown()
        {
            SocketUtil.ShutdownSocket(Socket);
            foreach (var listener in _serverScoketEventListeners)
            {
                try
                {
                    listener.OnServerShutDown(this);
                }
                catch (Exception ex)
                {
                    Log<ServerSocket>.Error(ex, string.Format("Notify socket server shut down has exception, name: {0}, listenerType: {1}", Name, listener.GetType().Name));
                }
            }
            ServerStateChanged(ServerSocketStateChangedState.Shutdown);
            Log<ServerSocket>.Info(string.Format("Socket server shutdown, name: {0}, listeningEndPoint: {1}.", Name, ListeningEndPoint));
        }

        private void ServerStateChanged(ServerSocketStateChangedState serverSocketStateChangedState)
        {
            var excuteAction = GetAction(serverSocketStateChangedState);
            foreach (var listener in _serverScoketEventListeners)
            {
                try
                {
                    excuteAction?.Invoke(listener, this);
                }
                catch (Exception ex)
                {
                    Log<ServerSocket>.Error(ex, string.Format("Notify socket server {0} has exception, name: {1}, listenerType: {2}", serverSocketStateChangedState, ToString(), Name, listener.GetType().Name));
                }
            }
        }

        private Action<IServerScoketEventListener, IServerSocket> GetAction(ServerSocketStateChangedState serverSocketStateChangedState)
        {
            switch (serverSocketStateChangedState)
            {
                case ServerSocketStateChangedState.Shutdown:
                    return (listener, server) => listener.OnServerShutDown(server);

                case ServerSocketStateChangedState.Started:
                    return (listener, server) => listener.OnServerStarted(server);

                default:
                    break;
            }
            return null;
        }

        private class ServerTcpConnectionEventListener : ITcpConnectionEventListener
        {
            private readonly ServerSocket _serverSocket;

            public ServerTcpConnectionEventListener(ServerSocket serverSocket)
            {
                _serverSocket = serverSocket;
            }

            public void OnConnectionAccepted(ITcpConnection connection)
            {
            }

            public void OnConnectionClosed(ITcpConnection connection, SocketError socketError)
            {
                _serverSocket.RemoveConnection(connection.Id);
            }

            public void OnConnectionEstablished(ITcpConnection connection)
            {
            }

            public void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError)
            {
            }
        }
    }
}