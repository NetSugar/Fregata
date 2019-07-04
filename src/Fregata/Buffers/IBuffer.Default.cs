using System;
using System.Runtime.InteropServices;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/5/30 17:41:56
    /// </summary>
    public class Buffer : IBuffer
    {
        private static long CurrentId = 0;

        private readonly int _size;
        private int _position;
        private int _using = 0;

        public Buffer(int size)
        {
            _size = size;
            Length = 0;
            _position = 0;
            FreeSpace = size;
            Data = new byte[size];
            GCHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Memory = new Memory<byte>(Data);
            Id = System.Threading.Interlocked.Increment(ref CurrentId);
        }

        public long Id { get; }
        public int Length { get; private set; }

        public int Postion
        {
            get => _position;
            set
            {
                _position = value;
                Eof = _position >= Length;
            }
        }

        public int FreeSpace { get; private set; }
        public bool Eof { get; private set; }
        public GCHandle GCHandle { get; }
        public byte[] Data { get; }
        public Memory<byte> Memory { get; }
        public IMemoryBlock NextMemory => Next;
        public IBuffer Next { get; set; }
        public IBufferPool Pool { get; set; }

        public Action<int> WriteAdvanceCompeleted { get; set; }

        public Action<int> ReadAdvanceCompeleted { get; set; }

        public long TotalBufferLength
        {
            get
            {
                return Length + (Next?.TotalBufferLength ?? 0);
            }
        }

        public void WriteAdvance(int bytes)
        {
            Length += bytes;
            _position += bytes;
            Eof = Length == _size;
            FreeSpace = _size - Length;
            WriteAdvanceCompeleted?.Invoke(bytes);
        }

        public int ReadFree(int count)
        {
            int read = Math.Min(Length - _position, count);
            ReadAdvance(read);
            return read;
        }

        public void SetLength(int length)
        {
            Length = length;
        }

        public unsafe int Write(byte[] buffer, int offset, int count)
        {
            int len = Math.Min(FreeSpace, count);
            if (len <= 8)
            {
                for (int i = 0; i < len; i++)
                {
                    Data[i + Postion] = buffer[offset + i];
                }
            }
            else
            {
                System.Buffer.BlockCopy(buffer, offset, Data, _position, len);
            }
            WriteAdvance(len);
            return len;
        }

        public int Write(ReadOnlyMemory<byte> buffer, int offset, int count)
        {
            int len = Math.Min(FreeSpace, count);
            if (len <= 8)
            {
                for (int i = 0; i < len; i++)
                {
                    Memory.Span[i + Postion] = buffer.Span[offset + i];
                }
            }
            else
            {
                buffer.Slice(offset, len).CopyTo(Memory.Slice(_position, len));
            }
            WriteAdvance(len);
            return len;
        }

        public void Write(byte data)
        {
            Data[_position] = data;
            WriteAdvance(1);
        }

        public unsafe bool TryWrite(short value)
        {
            int length = 2;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryWrite(int value)
        {
            int length = 4;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryWrite(long value)
        {
            int length = 8;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryWrite(ushort value)
        {
            int length = 2;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryWrite(uint value)
        {
            int length = 4;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryWrite(ulong value)
        {
            int length = 8;
            if (FreeSpace >= length)
            {
                BitHelper.Write(Data, _position, value);
                WriteAdvance(length);
                return true;
            }
            return false;
        }

        public bool TryGetWriteMemory(int size, out Span<byte> buffer)
        {
            buffer = null;
            if (FreeSpace >= size)
            {
                buffer = Memory.Span.Slice(_position, size);
                return true;
            }
            return false;
        }

        public Memory<byte> GetWriteMemory(int size)
        {
            if (FreeSpace > size)
            {
                return Memory.Slice(_position, size);
            }
            else
            {
                return Memory.Slice(_position, FreeSpace);
            }
        }

        public Memory<byte> GetWriteMemory()
        {
            return Memory.Slice(_position, FreeSpace);
        }

        public Memory<byte> AllocateWriteMemory(int bytes)
        {
            Memory<byte> result = GetWriteMemory(bytes);
            WriteAdvance(result.Length);
            return result;
        }

        public Span<byte> GetSpan(int size)
        {
            if (FreeSpace > size)
            {
                return Memory.Span.Slice(_position, size);
            }
            else
            {
                return Memory.Span.Slice(_position, FreeSpace);
            }
        }

        public bool TryGetSpan(int size, out Span<byte> result)
        {
            result = null;
            if (FreeSpace >= size)
            {
                result = Memory.Span.Slice(_position, size);
                return true;
            }
            return false;
        }

        public Span<byte> GetSpan()
        {
            return Memory.Span.Slice(_position, FreeSpace);
        }

        public Span<byte> AllocateSpan(int bytes)
        {
            Span<byte> result = GetSpan(bytes);
            WriteAdvance(result.Length);
            return result;
        }

        public bool TryAllocateSpan(int size, out Span<byte> result)
        {
            result = null;
            if (FreeSpace >= size)
            {
                result = Memory.Span.Slice(_position, size);
                WriteAdvance(size);
                return true;
            }
            return false;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            Span<byte> source = Read(count);
            if (source.Length <= 8)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    buffer[offset + i] = source[i];
                }
            }
            else
            {
                Span<byte> dest = new Span<byte>(buffer, offset, source.Length);
                source.CopyTo(dest);
            }
            return source.Length;
        }

        public int Read(Memory<byte> memory, int offset, int count)
        {
            Span<byte> source = Read(count);
            if (source.Length <= 8)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    memory.Span[offset + i] = source[i];
                }
            }
            else
            {
                source.CopyTo(memory.Span.Slice(offset, source.Length));
            }
            return source.Length;
        }

        public Memory<byte> GetReadMemory(int size)
        {
            var len = Math.Min(size, Length);
            var memory = Memory.Slice(_position, len);
            ReadAdvance(len);
            return memory;
        }

        public byte Read()
        {
            byte result = Data[_position];
            ReadAdvance(1);
            return result;
        }

        public unsafe bool TryRead(out short value)
        {
            value = 0;
            int length = 2;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadInt16(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryRead(out int value)
        {
            value = 0;
            int length = 4;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadInt32(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryRead(out long value)
        {
            value = 0;
            int length = 8;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadInt64(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryRead(out ushort value)
        {
            value = 0;
            int length = 2;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadUInt16(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryRead(out uint value)
        {
            value = 0;
            int length = 4;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadUInt32(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public unsafe bool TryRead(out ulong value)
        {
            value = 0;
            int length = 8;
            if (Length - _position >= length)
            {
                value = BitHelper.ReadUInt64(Data, _position);
                ReadAdvance(length);
                return true;
            }
            return false;
        }

        public Span<byte> Read(int size)
        {
            int space = Math.Min(Length - _position, size);
            var result = Memory.Span.Slice(_position, space);
            ReadAdvance(space);
            return result;
        }

        public void ReadAdvance(int bytes)
        {
            _position += bytes;
            Eof = Length == _position;
            ReadAdvanceCompeleted?.Invoke(bytes);
        }

        public IBuffer RegisterWriteAdvanceCompeleted(Action<int> action)
        {
            if (WriteAdvanceCompeleted == null)
                WriteAdvanceCompeleted = action;
            return this;
        }

        public IBuffer RegisterReadAdvanceCompeleted(Action<int> action)
        {
            if (ReadAdvanceCompeleted == null)
                ReadAdvanceCompeleted = action;
            return this;
        }

        public void Reset()
        {
            Length = 0;
            _position = 0;
            FreeSpace = _size;
            Next = null;
            System.Threading.Interlocked.Exchange(ref _using, 1);
        }

        public void Free()
        {
            if (Next != null) Next.Free();
            if (System.Threading.Interlocked.CompareExchange(ref _using, 0, 1) == 1)
            {
                ReadAdvanceCompeleted = null;
                WriteAdvanceCompeleted = null;
                if (Pool != null)
                    Pool.Push(this);
            }
        }

        public void Dispose()
        {
            if (GCHandle.IsAllocated)
            {
                GCHandle.Free();
            }
        }
    }
}