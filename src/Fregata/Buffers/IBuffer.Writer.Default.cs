using System;
using System.Collections.Generic;
using System.Text;

namespace Fregata.Buffers
{
    internal class BufferWriter : Disposable, IBufferWriter
    {
        private readonly IBufferPool _bufferPool;
        private readonly byte[] _cacheBlock;
        private readonly int _subStringLen;
        private readonly int _maxCharBytes;
        private IBuffer _writerBuffer;
        private IBuffer _writerLastBuffer;

        public BufferWriter(IBufferPool bufferPool, bool littelEndian, Encoding coding)
        {
            _bufferPool = bufferPool;
            LittleEndian = littelEndian;
            Encoding = coding;
            _subStringLen = BufferOptions.Default.CacheBlockLen / Encoding.GetMaxByteCount(1);
            _maxCharBytes = Encoding.GetMaxByteCount(1);
            _cacheBlock = new byte[BufferOptions.Default.CacheBlockLen];
        }

        public long Length { get; private set; }

        public Encoding Encoding { get; }

        public bool LittleEndian { get; }

        public Action<IBuffer, IBuffer> FlushCompleted { get; set; }

        private IBuffer Writer
        {
            get
            {
                if (_writerLastBuffer == null)
                {
                    var result = _bufferPool.Pop().RegisterWriteAdvanceCompeleted((size) => { Length += size; });
                    _writerBuffer = result;
                    _writerLastBuffer = result;
                    result = null;
                }
                else
                {
                    if (_writerLastBuffer.Eof)
                    {
                        var result = _bufferPool.Pop().RegisterWriteAdvanceCompeleted((size) => { Length += size; });
                        _writerLastBuffer.Next = result;
                        _writerLastBuffer = result;
                        result = null;
                    }
                }
                return _writerLastBuffer;
            }
        }

        public MemoryBlockCollection Allocate(int size)
        {
            List<Memory<byte>> blocks = new List<Memory<byte>>();
            while (size > 0)
            {
                Memory<byte> item = Writer.AllocateWriteMemory(size);
                blocks.Add(item);
                size -= item.Length;
                if (size == 0)
                    break;
            }
            return new MemoryBlockCollection(blocks);
        }

        public void Flush()
        {
            FlushCompleted?.Invoke(_writerBuffer, _writerLastBuffer);
            Length = 0;
            _writerBuffer = null;
            _writerLastBuffer = null;
        }

        public Memory<byte> GetMemory()
        {
            return Writer.GetWriteMemory();
        }

        public Memory<byte> GetMemory(int size)
        {
            return Writer.GetWriteMemory(size);
        }

        public void Write(bool value)
        {
            if (value)
            {
                Writer.Write((byte)1);
            }
            else
            {
                Writer.Write((byte)0);
            }
        }

        public void Write(short value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapInt16(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 2);
            }
        }

        public void Write(int value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapInt32(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 4);
            }
        }

        public void Write(long value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapInt64(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 8);
            }
        }

        public void Write(ushort value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapUInt16(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 2);
            }
        }

        public void Write(uint value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapUInt32(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 4);
            }
        }

        public void Write(ulong value)
        {
            if (!LittleEndian)
            {
                value = BitHelper.SwapUInt64(value);
            }
            if (!Writer.TryWrite(value))
            {
                BitHelper.Write(_cacheBlock, 0, value);
                Write(_cacheBlock, 0, 8);
            }
        }

        public void Write(DateTime value)
        {
            Write(value.Ticks);
        }

        public void Write(char value)
        {
            Write((short)value);
        }

        public unsafe void Write(float value)
        {
            int num = *(int*)&value;
            Write(num);
        }

        public unsafe void Write(double value)
        {
            long num = *(long*)(&value);
            Write(num);
        }

        public int Write(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            int cvalueLen = value.Length;
            int index = 0;
            int count = 0;
            int ensize = value.Length * _maxCharBytes;
            IBuffer buffer = Writer;
            if (buffer.FreeSpace > ensize)
            {
                var len = Encoding.GetBytes(value, index, value.Length, buffer.Data, buffer.Postion);
                buffer.WriteAdvance(len);
                return len;
            }
            while (cvalueLen > 0)
            {
                int encodingLen;
                if (cvalueLen > _subStringLen)
                    encodingLen = _subStringLen;
                else
                    encodingLen = cvalueLen;
                var len = Encoding.GetBytes(value, index, encodingLen, _cacheBlock, 0);
                count += len;
                cvalueLen -= encodingLen;
                index += encodingLen;
                Write(_cacheBlock, 0, len);
            }
            return count;
        }

        public int Write(string value, params object[] parameters)
        {
            return Write(string.Format(value, parameters));
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int len = Writer.Write(buffer, offset, count);
                offset += len;
                count -= len;
            }
        }

        public void Write(ReadOnlyMemory<byte> data)
        {
            int count = data.Length;
            int offset = 0;
            while (count > 0)
            {
                int len = Writer.Write(data, offset, count);
                offset += len;
                count -= len;
            }
        }

        public void WriteAdvance(int bytes)
        {
            Writer.WriteAdvance(bytes);
        }

        public void WriteStringWithLength(string value)
        {
            MemoryBlockCollection? mbc = null;
            if (!Writer.TryAllocateSpan(4, out Span<byte> header))
            {
                mbc = Allocate(4);
            }
            int len = Write(value);
            if (!LittleEndian)
                len = BitHelper.SwapInt32(len);
            if (mbc != null)
            {
                mbc.Value.Full(len);
            }
            else
            {
                BitHelper.Write(header, len);
            }
        }

        public void WriteStringWithLength(string value, params object[] parameters)
        {
            WriteStringWithLength(string.Format(value, parameters));
        }

        public void WriteStringWithShortLength(string value)
        {
            MemoryBlockCollection? mbc = null;
            if (!Writer.TryAllocateSpan(2, out Span<byte> header))
            {
                mbc = Allocate(2);
            }
            short len = (short)Write(value);
            if (!LittleEndian)
                len = BitHelper.SwapInt16(len);
            if (mbc != null)
            {
                mbc.Value.Full(len);
            }
            else
            {
                BitHelper.Write(header, len);
            }
        }

        public void WriteStringWithShortLength(string value, params object[] parameters)
        {
            WriteStringWithShortLength(string.Format(value, parameters));
        }

        protected override void DisposeCode()
        {
            if (_writerBuffer != null)
            {
                _writerBuffer.Free();
            }
            if (_writerLastBuffer != null)
            {
                _writerLastBuffer.Free();
            }
        }
    }
}