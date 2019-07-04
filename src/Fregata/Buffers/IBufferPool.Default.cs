using Fregata.Exceptions;
using System.Collections.Concurrent;
using System.Threading;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/5/30 18:05:48
    /// </summary>
    public class BufferPool : Disposable, IBufferPool
    {
        private readonly ConcurrentQueue<IBuffer> _pool = new ConcurrentQueue<IBuffer>();
        private readonly IBufferCreater _bufferCreater;
        private readonly int _size;
        private readonly int _initialCount;
        private int _totalCount;
        private int _availableCount;

        public BufferPool(int size, int initialCount, IBufferCreater itemCreater)
        {
            if (initialCount < 0) throw new FregataException("buffer pool initial count must be more than 0.");
            if (size < 0) throw new FregataException("buffer size must be more than 0.");
            _size = size;
            _initialCount = initialCount;
            _bufferCreater = itemCreater ?? throw new FregataException("buffer creater must not be null.");
            Init();
        }

        private void Init()
        {
            IBuffer item;
            for (int i = 0; i < _initialCount; i++)
            {
                item = CreateBuffer();
                PushBuffer(item);
            }
        }

        private IBuffer CreateBuffer()
        {
            _totalCount = Interlocked.Increment(ref _totalCount);
            IBuffer item = _bufferCreater.Create(this, _size);
            return item;
        }

        public int TotalCount => _totalCount;

        public int AvailableCount => _availableCount;

        public IBuffer Pop()
        {
            if (!_pool.TryDequeue(out IBuffer item))
            {
                item = CreateBuffer();
            }
            else
            {
                Interlocked.Decrement(ref _availableCount);
            }
            item.Reset();
            return item;
        }

        public void Push(IBuffer item)
        {
            if (_availableCount > _initialCount)
            {
                Interlocked.Decrement(ref _totalCount);
                item.Dispose();
            }
            else
            {
                PushBuffer(item);
            }
        }

        private void PushBuffer(IBuffer item)
        {
            _pool.Enqueue(item);
            Interlocked.Increment(ref _availableCount);
        }

        protected override void DisposeCode()
        {
            while (true)
            {
                if (_pool.TryDequeue(out IBuffer buffer))
                {
                    buffer.Dispose();
                }
                else
                    break;
            }
            _totalCount = _availableCount = 0;
        }
    }
}