namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/3 16:11:21
    /// </summary>
    public interface IBufferPoolCreater
    {
        IBufferPool Create(int size, int initialCount);
    }
}