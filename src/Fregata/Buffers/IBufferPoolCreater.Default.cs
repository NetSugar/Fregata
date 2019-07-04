namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/3 16:13:15
    /// </summary>
    public class BufferPoolCreater : IBufferPoolCreater
    {
        public IBufferPool Create(int size, int initialCount)
        {
            return new BufferPool(size, initialCount, new BufferCreater());
        }
    }
}