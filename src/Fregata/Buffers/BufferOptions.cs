namespace Fregata.Buffers
{
    internal readonly struct BufferOptions
    {
        public int CacheBlockLen { get; }

        public int BufferSize { get; }

        public int BufferInitialCount { get; }

        public BufferOptions(int cacheBlockLen, int buffseSize, int bufferInitialCount)
        {
            CacheBlockLen = cacheBlockLen;
            BufferSize = buffseSize;
            BufferInitialCount = bufferInitialCount;
        }

        private static BufferOptions DefaultValue = new BufferOptions(512, 1024, 1024 * 4);

        internal static ref BufferOptions Default => ref DefaultValue;
    }
}