using System;
using System.Text;

namespace Fregata.Buffers
{
    public interface IBufferReader : IDisposable
    {
        long Length { get; }
        bool LittleEndian { get; }
        Decoder Decoder { get; }

        bool TryRead(int count);

        byte[] Read(int count);

        ReadResult ReadResult();

        ReadResult ReadResult(int count);

        int Read(IBufferWriter bufferWriter, int count);

        int Read(Memory<byte> memory);

        int Read(Memory<byte> memory, int offset, int count);

        int Read(ArraySegment<byte> data);

        int Read(byte[] buffer, int offset, int count);

        byte ReadByte();

        bool ReadBool();

        short ReadInt16();

        int ReadInt32();

        long ReadInt64();

        ushort ReadUInt16();

        uint ReadUInt32();

        ulong ReadUInt64();

        char ReadChar();

        DateTime ReadDateTime();

        float ReadFloat();

        double ReadDouble();

        string ReadString(int length);

        string ReadString(long length);

        bool TryReadWith(string eof, out string value, bool returnEof = false);

        bool TryReadWith(byte[] eof, out string value, bool returnEof = false);

        bool TryReadWith(byte[] eof, out byte[] value, bool returnEof = false);

        string ReadToEnd();

        void Import(IBuffer buffer, IBuffer lastBuffer);
    }
}