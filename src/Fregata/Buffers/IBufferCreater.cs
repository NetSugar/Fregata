namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/5/31 13:43:51
    /// </summary>
    public interface IBufferCreater
    {
        IBuffer Create(IBufferPool bufferPool,int size);
    }
}