using System;
using System.Runtime.InteropServices;

namespace Fregata.Buffers
{
    public interface IBuffer : IMemoryBlock, IDisposable
    {
        bool Eof { get; }
        int FreeSpace { get; }
        GCHandle GCHandle { get; }
        IBuffer Next { get; set; }
        IBufferPool Pool { get; set; }
        int Postion { get; set; }
        long TotalBufferLength { get; }
        Action<int> WriteAdvanceCompeleted { get; set; }

        Action<int> ReadAdvanceCompeleted { get; set; }

        IBuffer RegisterWriteAdvanceCompeleted(Action<int> action);

        IBuffer RegisterReadAdvanceCompeleted(Action<int> action);

        Memory<byte> AllocateWriteMemory(int bytes);

        Span<byte> AllocateSpan(int bytes);

        void Free();

        Memory<byte> GetWriteMemory();

        Memory<byte> GetWriteMemory(int size);

        Span<byte> GetSpan();

        Span<byte> GetSpan(int size);

        byte Read();

        int Read(byte[] buffer, int offset, int count);

        int Read(Memory<byte> memory, int offset, int count);

        Memory<byte> GetReadMemory(int size);

        Span<byte> Read(int size);

        void ReadAdvance(int bytes);

        int ReadFree(int count);

        void Reset();

        void SetLength(int length);

        bool TryAllocateSpan(int size, out Span<byte> result);

        bool TryGetWriteMemory(int size, out Span<byte> buffer);

        bool TryGetSpan(int size, out Span<byte> result);

        bool TryRead(out int value);

        bool TryRead(out long value);

        bool TryRead(out short value);

        bool TryRead(out uint value);

        bool TryRead(out ulong value);

        bool TryRead(out ushort value);

        bool TryWrite(int value);

        bool TryWrite(long value);

        bool TryWrite(short value);

        bool TryWrite(uint value);

        bool TryWrite(ulong value);

        bool TryWrite(ushort value);

        void Write(byte data);

        int Write(byte[] buffer, int offset, int count);

        int Write(ReadOnlyMemory<byte> buffer, int offset, int count);

        void WriteAdvance(int bytes);
    }
}