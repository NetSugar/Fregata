using System;
using System.Text;

namespace Fregata.Buffers
{
    public class BufferPipeline : Disposable, IDisposable
    {
        private readonly IBufferPool _bufferPool;
        private readonly bool _isNeedDisposePool;

        public BufferPipeline() : this(true, Encoding.UTF8, null, null)
        {
        }

        public BufferPipeline(IBufferPool bufferPool) : this(true, Encoding.UTF8, null, bufferPool)
        {
        }

        public BufferPipeline(Action<IBuffer, IBuffer> writerFlushCompleted) : this(true, Encoding.UTF8, writerFlushCompleted, null)
        {
        }

        public BufferPipeline(Action<IBuffer, IBuffer> writerFlushCompleted, IBufferPool bufferPool) : this(true, Encoding.UTF8, writerFlushCompleted, bufferPool)
        {
        }

        public BufferPipeline(bool littelEndian, Encoding coding, Action<IBuffer, IBuffer> writerFlushCompleted, IBufferPool bufferPool)
        {
            if (bufferPool == null)
            {
                _isNeedDisposePool = true;
                _bufferPool = new BufferPoolCreater().Create(BufferOptions.Default.BufferSize, BufferOptions.Default.BufferInitialCount);
            }
            else
            {
                _bufferPool = bufferPool;
            }
            Writer = new BufferWriter(_bufferPool, littelEndian, coding);
            Reader = new BufferReader(littelEndian, coding);
            if (writerFlushCompleted == null)
            {
                Writer.FlushCompleted = (first, last) => Reader.Import(first, last);
            }
            else
            {
                Writer.FlushCompleted = writerFlushCompleted;
            }
        }

        public IBufferWriter Writer { get; }

        public IBufferReader Reader { get; }

        protected override void DisposeCode()
        {
            Reader?.Dispose();
            Writer?.Dispose();
            if (_isNeedDisposePool)
            {
                _bufferPool.Dispose();
            }
        }
    }
}