using System;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/12 15:53:14
    /// </summary>
    public interface IMemoryBlock
    {
        long Id { get; }

        byte[] Data { get; }

        Memory<byte> Memory { get; }

        int Length { get; }

        IMemoryBlock NextMemory { get; }
    }
}