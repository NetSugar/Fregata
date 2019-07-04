using Fregata.Buffers;
using Fregata.Framing;
using Fregata.Options;
using Fregata.Sockets.Handlers;
using Fregata.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Fregata.Sockets
{
    public class TcpConnection : ITcpConnection
    {
        private int _receiving;
        private int _sending;
        private int _parsing;
        private int _closing;
        private readonly IMessageFramer _messageFramer;

        private readonly BufferPipeline _receiveBuufferPipeline;

        private long _pendingMessageCount = 0L;
        private ConcurrentQueue<ReadOnlyMemory<byte>> _sendingQueue = new ConcurrentQueue<ReadOnlyMemory<byte>>();
        private readonly BufferPipeline _sendBuufferPipeline;

        private readonly ITcpConnectionHandler _tcpConnectionHandler;
        private readonly IList<ITcpConnectionEventListener> _tcpConnectionEventListeners;

        public TcpConnection(string name, Socket socket, FregataOptions fregataOptions, IBufferPool receivedBufferPool, IBufferPool sendBufferPool, ITcpConnectionHandler tcpConnectionHandler)
        {
            Id = SnowflakeId.Default().NextId();
            Name = name;
            Socket = socket;
            LocalEndPoint = socket.LocalEndPoint;
            RemotingEndPoint = socket.RemoteEndPoint;
            Setting = fregataOptions;
            _tcpConnectionEventListeners = new List<ITcpConnectionEventListener>();

            _tcpConnectionHandler = tcpConnectionHandler;

            _messageFramer = new LengthPrefixMessageFramer(Setting);
            _messageFramer.RegisterMessageArrivedCallback(OnMessageArrived);
            _receiveBuufferPipeline = new BufferPipeline(bufferPool: receivedBufferPool, littelEndian: fregataOptions.LittleEndian, coding: fregataOptions.Encode, writerFlushCompleted: null);

            _sendBuufferPipeline = new BufferPipeline(bufferPool: sendBufferPool, littelEndian: fregataOptions.LittleEndian, coding: fregataOptions.Encode, writerFlushCompleted: null);
            Task.Run(() => TryReceiveAsync());
        }

        public long Id { get; }
        public string Name { get; }
        public Socket Socket { get; private set; }
        public bool IsConnected => Socket != null && Socket.Connected;
        public EndPoint LocalEndPoint { get; }
        public EndPoint RemotingEndPoint { get; }
        public FregataOptions Setting { get; }

        public long PendingMessageCount
        {
            get { return _pendingMessageCount; }
        }

        #region Received

        public void RegisterConnectionEventListener(ITcpConnectionEventListener listener)
        {
            _tcpConnectionEventListeners.Add(listener);
        }

        public void RegisterConnectionEventListener(IList<ITcpConnectionEventListener> listeners)
        {
            foreach (var listener in listeners)
            {
                RegisterConnectionEventListener(listener);
            }
        }

        public void Accepted()
        {
            foreach (var listener in _tcpConnectionEventListeners)
            {
                try
                {
                    listener.OnConnectionAccepted(this);
                }
                catch (Exception ex)
                {
                    Log<TcpConnection>.Error(ex, string.Format("Notify socket server new client connection accepted has exception, name: {0}, listenerType: {1}", Name, listener.GetType().Name));
                }
            }
        }

        private async Task TryReceiveAsync()
        {
            if (!EnterReceiving()) return;
            try
            {
                var memory = _receiveBuufferPipeline.Writer.GetMemory();
                var receivedCount = await Socket.ReceiveAsync(memory);
                if (receivedCount == 0)
                {
                    CloseInternal(SocketError.Shutdown, "Socket normal close.", null);
                }
                _receiveBuufferPipeline.Writer.WriteAdvance(receivedCount);
                _receiveBuufferPipeline.Writer.Flush();
                TryParsingReceived();
            }
            catch (Exception ex)
            {
                _receiveBuufferPipeline.Writer.Flush();
                CloseInternal(SocketError.OperationNotSupported, "Socket receive error.", ex);
                return;
            }
            ExitReceiving();
            await TryReceiveAsync();
        }

        private void TryParsingReceived()
        {
            if (!EnterParsing()) return;

            try
            {
                while (_messageFramer.CanUnFrameData(_receiveBuufferPipeline.Reader))
                {
                    _messageFramer.UnFrameData(_receiveBuufferPipeline.Reader);
                }
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Parsing received data error.", ex);
                return;
            }
            finally
            {
                ExitParsing();
            }
        }

        private void OnMessageArrived(ReadResult readResult)
        {
            try
            {
                _tcpConnectionHandler.OnMessageArrived(this, readResult);
            }
            catch (Exception ex)
            {
                Log<TcpConnection>.Error(ex, string.Format("TCP connection call message arrived handler has exception, name: {0}", Name));
            }
        }

        private bool EnterReceiving()
        {
            return Interlocked.CompareExchange(ref _receiving, 1, 0) == 0;
        }

        private void ExitReceiving()
        {
            Interlocked.Exchange(ref _receiving, 0);
        }

        private bool EnterParsing()
        {
            return Interlocked.CompareExchange(ref _parsing, 1, 0) == 0;
        }

        private void ExitParsing()
        {
            Interlocked.Exchange(ref _parsing, 0);
        }

        #endregion Received

        #region Send

        public void Send(byte[] message)
        {
            if (message == null || message.Length == 0)
            {
                return;
            }
            _sendingQueue.Enqueue(new ReadOnlyMemory<byte>(message));
            Interlocked.Increment(ref _pendingMessageCount);

            Task.Run(() => TrySendAsync());
        }

        private async ValueTask TrySendAsync()
        {
            if (_closing == 1) return;
            if (!EnterSending()) return;
            while (_sendingQueue.TryDequeue(out ReadOnlyMemory<byte> data))
            {
                Interlocked.Decrement(ref _pendingMessageCount);
                _messageFramer.FrameData(_sendBuufferPipeline.Writer, data);
                if (_sendBuufferPipeline.Writer.Length >= Setting.SendMaxPacketSize)
                {
                    break;
                }
            }
            if (_sendBuufferPipeline.Writer.Length > 0)
            {
                _sendBuufferPipeline.Writer.Flush();
            }

            if (_sendBuufferPipeline.Reader.Length == 0)
            {
                ExitSending();
                if (_sendingQueue.Count > 0)
                {
                    await TrySendAsync();
                }
                return;
            }

            try
            {
                var readResult = _sendBuufferPipeline.Reader.ReadResult();
                await Socket.SendAsync(readResult.Buffer, SocketFlags.None);
            }
            catch (Exception ex)
            {
                CloseInternal(SocketError.Shutdown, "Socket send error, errorMessage:" + ex.Message, ex);
            }
            ExitSending();
            await TrySendAsync();
        }

        private bool EnterSending()
        {
            return Interlocked.CompareExchange(ref _sending, 1, 0) == 0;
        }

        private void ExitSending()
        {
            Interlocked.Exchange(ref _sending, 0);
        }

        #endregion Send

        public void Close()
        {
            CloseInternal(SocketError.Success, "Socket normal close.", null);
        }

        private void CloseInternal(SocketError socketError, string reason, Exception exception)
        {
            if (Interlocked.CompareExchange(ref _closing, 1, 0) == 0)
            {
                SocketUtil.ShutdownSocket(Socket);
                var isDisposedException = exception != null && exception is ObjectDisposedException;
                if (!isDisposedException)
                {
                    Log<TcpConnection>.Info(string.Format("TCP connection closed, name: {0}, remoteEndPoint: {1}, socketError: {2}, reason: {3}, ex: {4}", Name, RemotingEndPoint, socketError, reason, exception));
                }
                Socket = null;

                try
                {
                    _tcpConnectionHandler.OnConnectionClosed(this, socketError);
                }
                catch (Exception ex)
                {
                    Log<TcpConnection>.Error(ex, string.Format("TCP connection closed handler execution has exception, name: {0}", Name));
                }

                foreach (var listener in _tcpConnectionEventListeners)
                {
                    try
                    {
                        listener.OnConnectionClosed(this, socketError);
                    }
                    catch (Exception ex)
                    {
                        Log<TcpConnection>.Error(ex, string.Format("Notify socket server client connection closed has exception, name: {0}, listenerType: {1}", Name, listener.GetType().Name));
                    }
                }
            }
        }
    }
}