using System;
using System.Text;

namespace Fregata.Buffers
{
    public interface IBufferWriter : IDisposable
    {
        long Length { get; }

        Encoding Encoding { get; }

        bool LittleEndian { get; }

        Memory<byte> GetMemory();

        Memory<byte> GetMemory(int size);

        void Write(bool value);

        void Write(short value);

        void Write(int value);

        void Write(long value);

        void Write(ushort value);

        void Write(uint value);

        void Write(ulong value);

        void Write(DateTime value);

        void Write(char value);

        void Write(float value);

        void Write(double value);

        int Write(string value);

        int Write(string value, params object[] parameters);

        void WriteStringWithShortLength(string value);

        void WriteStringWithShortLength(string value, params object[] parameters);

        void WriteStringWithLength(string value);

        void WriteStringWithLength(string value, params object[] parameters);

        void Flush();

        Action<IBuffer, IBuffer> FlushCompleted { get; set; }

        MemoryBlockCollection Allocate(int size);

        void Write(byte[] buffer, int offset, int count);

        void Write(ReadOnlyMemory<byte> data);

        void WriteAdvance(int bytes);
    }
}