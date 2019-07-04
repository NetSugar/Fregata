using System;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/5/30 18:04:50
    /// </summary>
    public interface IBufferPool : IDisposable
    {
        int TotalCount { get; }

        int AvailableCount { get; }

        IBuffer Pop();

        void Push(IBuffer item);
    }
}