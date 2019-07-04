using Fregata.Buffers;
using Fregata.Options;
using Fregata.Sockets.Handlers;
using Fregata.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Fregata.Sockets.Clients
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/3 15:31:11
    /// </summary>
    public class ClientSocket : IClientSocket
    {
        private readonly IBufferPool _receivedBufferPool;
        private readonly IBufferPool _sendBufferPool;
        private readonly ITcpConnectionHandler _tcpConnectionHandler;
        private readonly ManualResetEvent _waitConnectHandle;
        private readonly IList<ITcpConnectionEventListener> _tcpConnectionEventListeners;

        public ClientSocket(string name, EndPoint serverEndPoint, FregataOptions fregataOptions, IBufferPool receivedBufferPool, IBufferPool sendBufferPool, ITcpConnectionHandler tcpConnectionHandler)
        {
            Id = SnowflakeId.Default().NextId();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ServerEndPoint = serverEndPoint ?? throw new ArgumentNullException(nameof(serverEndPoint));
            Setting = fregataOptions ?? throw new ArgumentNullException(nameof(fregataOptions));
            _receivedBufferPool = receivedBufferPool ?? throw new ArgumentNullException(nameof(receivedBufferPool));
            _sendBufferPool = sendBufferPool ?? throw new ArgumentNullException(nameof(sendBufferPool));
            _waitConnectHandle = new ManualResetEvent(false);
            Socket = SocketUtil.CreateSocket(serverEndPoint, fregataOptions.SendMaxSize, fregataOptions.ReceiveMaxSize);
            _tcpConnectionHandler = tcpConnectionHandler;
            _tcpConnectionEventListeners = new List<ITcpConnectionEventListener>();
        }

        public long Id { get; }
        public string Name { get; }
        public EndPoint ServerEndPoint { get; }
        public EndPoint LocalEndPoint { get; private set; }
        public FregataOptions Setting { get; }
        public Socket Socket { get; private set; }
        public bool IsConnected { get { return Connection != null && Connection.IsConnected; } }
        public TcpConnection Connection { get; private set; }


        public IClientSocket RegisterConnectionEventListener(ITcpConnectionEventListener listener)
        {
            _tcpConnectionEventListeners.Add(listener);
            return this;
        }

        public IClientSocket Start()
        {
            var socketArgs = new SocketAsyncEventArgs
            {
                AcceptSocket = Socket,
                RemoteEndPoint = ServerEndPoint
            };
            socketArgs.Completed += OnConnectAsyncCompleted;

            var firedAsync = Socket.ConnectAsync(socketArgs);
            if (!firedAsync)
            {
                ProcessConnect(socketArgs);
            }

            _waitConnectHandle.WaitOne(Setting.ClientConnectionWaitMilliseconds);

            if (Connection == null)
            {
                throw new Exception(string.Format("Client socket connect failed or timeout, name: {0}, serverEndPoint: {1}", Name, ServerEndPoint));
            }

            return this;
        }

        private void OnConnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            e.Completed -= OnConnectAsyncCompleted;
            e.AcceptSocket = null;
            e.RemoteEndPoint = null;
            e.Dispose();

            if (e.SocketError != SocketError.Success)
            {
                SocketUtil.ShutdownSocket(Socket);
                Log<ClientSocket>.Error(string.Format("Client socket connect failed, name: {0}, remotingServerEndPoint: {1}, socketError: {2}", Name, ServerEndPoint, e.SocketError));
                OnConnectionFailed(ServerEndPoint, e.SocketError);
                _waitConnectHandle.Set();
                return;
            }

            Connection = new TcpConnection(Name, Socket, Setting, _receivedBufferPool, _sendBufferPool, _tcpConnectionHandler);
            Connection.RegisterConnectionEventListener(_tcpConnectionEventListeners);
            LocalEndPoint = Connection.LocalEndPoint;
            Log<ClientSocket>.Info(string.Format("Client socket connected, name: {0}, remotingServerEndPoint: {1}, localEndPoint: {2}", Name, Connection.RemotingEndPoint, Connection.LocalEndPoint));

            OnConnectionEstablished(Connection);
        }

        private void OnConnectionEstablished(ITcpConnection connection)
        {
            foreach (var listener in _tcpConnectionEventListeners)
            {
                try
                {
                    listener.OnConnectionEstablished(connection);
                }
                catch (Exception ex)
                {
                    Log<ClientSocket>.Error(ex, string.Format("Client socket notify connection established has exception, name: {0}, listenerType: {1}", Name, listener.GetType().Name));
                }
            }
        }

        private void OnConnectionFailed(EndPoint remotingEndPoint, SocketError socketError)
        {
            foreach (var listener in _tcpConnectionEventListeners)
            {
                try
                {
                    listener.OnConnectionFailed(remotingEndPoint, socketError);
                }
                catch (Exception ex)
                {
                    Log<ClientSocket>.Error(ex, string.Format("Client socket notify connection failed has exception, name: {0}, listenerType: {1}", Name, listener.GetType().Name));
                }
            }
        }
    }
}