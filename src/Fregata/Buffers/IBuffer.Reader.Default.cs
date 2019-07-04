using Fregata.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fregata.Buffers
{
    internal class BufferReader : Disposable, IBufferReader
    {
        private readonly byte[] _cacheBlock;
        private readonly char[] _charCacheBlock;
        private readonly int _cacheBlockLength;
        private long _length;
        private IBuffer _readerBuffer;
        private IBuffer _readerLastBuffer;
        private ReadTaskCompletionSource _readCompletionSource;
        private readonly ConcurrentQueue<Tuple<IBuffer, IBuffer>> bufferQueue = new ConcurrentQueue<Tuple<IBuffer, IBuffer>>();

        public BufferReader(bool littelEndian, Encoding coding)
        {
            LittleEndian = littelEndian;
            Encoding = coding;
            Decoder = coding.GetDecoder();
            _cacheBlockLength = BufferOptions.Default.CacheBlockLen;
            SubStringLen = _cacheBlockLength / coding.GetMaxByteCount(1);
            MaxCharBytes = coding.GetMaxByteCount(1);
            _cacheBlock = new byte[_cacheBlockLength];
            _charCacheBlock = new char[_cacheBlockLength];
        }

        public long Length => _length;

        public bool LittleEndian { get; }

        public Encoding Encoding { get; }

        public Decoder Decoder { get; }

        public int SubStringLen { get; }

        public int MaxCharBytes { get; }

        private IBuffer GetReadBuffer()
        {
            if (_readerBuffer == null)
            {
                UpdateReadBuffer();
                if (_readerBuffer == null) return null;
            }
            if (_readerBuffer.Eof)
            {
                UpdateReadBuffer();
                IBuffer buf = _readerBuffer;
                _readerBuffer = _readerBuffer.Next;
                _readerBuffer.RegisterReadAdvanceCompeleted(size =>
                {
                    Interlocked.Add(ref _length, -1 * size);
                });
                _readerBuffer.Postion = 0;
                buf.Next = null;
                buf.Free();
            }

            return _readerBuffer;
        }

        private void UpdateReadBuffer()
        {
            while (bufferQueue.TryDequeue(out var result))
            {
                if (_readerLastBuffer == null)
                {
                    _readerBuffer = result.Item1;
                    _readerLastBuffer = result.Item2;
                    _readerBuffer.Postion = 0;
                    _readerBuffer.RegisterReadAdvanceCompeleted(size =>
                    {
                        Interlocked.Add(ref _length, -1 * size);
                    });
                }
                else
                {
                    _readerLastBuffer.Next = result.Item1;
                    if (result.Item2.Next == null)
                        _readerLastBuffer = result.Item2;
                    else
                        _readerLastBuffer = result.Item2.Next;
                }
            }
        }

        private IBuffer GetAndVerifyReadBuffer()
        {
            IBuffer result = GetReadBuffer();
            if (result == null)
                throw new NullReferenceException("buffer no data!");
            return result;
        }

        public void Import(IBuffer buffer, IBuffer lastBuffer)
        {
            Interlocked.Add(ref _length, buffer.TotalBufferLength);
            bufferQueue.Enqueue(Tuple.Create(buffer, lastBuffer));

            if (_readCompletionSource != null)
            {
                int len = Read(_readCompletionSource.Buffer, _readCompletionSource.Offset, _readCompletionSource.Count);
                _readCompletionSource.TrySetResult(len);
                _readCompletionSource = null;
            }
        }

        public int Read(IBufferWriter bufferWriter, int count)
        {
            int recount = 0;
            IBuffer sbuffer = GetReadBuffer();
            int offset = 0;
            Memory<byte> memory = null;
            while (sbuffer != null)
            {
                memory = bufferWriter.GetMemory(count);
                int rc = Read(memory);
                offset += rc;
                count -= rc;
                recount += rc;
                bufferWriter.WriteAdvance(rc);
                if (count == 0)
                    break;
                sbuffer = GetReadBuffer();
            }
            return recount;
        }

        public int Read(Memory<byte> memory)
        {
            return Read(memory, 0, memory.Length);
        }

        public int Read(Memory<byte> memory, int offset, int count)
        {
            int recount = 0;
            IBuffer sbuffer = GetReadBuffer();
            while (sbuffer != null)
            {
                int rc = sbuffer.Read(memory, offset, count);
                offset += rc;
                count -= rc;
                recount += rc;
                if (count == 0)
                    break;
                sbuffer = GetReadBuffer();
            }
            return recount;
        }

        public int Read(ArraySegment<byte> data)
        {
            return Read(data.Array, data.Offset, data.Count);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int recount = 0;
            IBuffer sbuffer = GetReadBuffer();
            while (sbuffer != null)
            {
                int rc = sbuffer.Read(buffer, offset, count);
                offset += rc;
                count -= rc;
                recount += rc;
                if (count == 0)
                    break;
                sbuffer = GetReadBuffer();
            }
            return recount;
        }

        public bool ReadBool()
        {
            CheckLength(1);
            IBuffer sbuffer = GetAndVerifyReadBuffer();
            return sbuffer.Read() != 0;
        }

        public byte ReadByte()
        {
            CheckLength(1);
            IBuffer sbuffer = GetAndVerifyReadBuffer();
            return sbuffer.Read();
        }

        public char ReadChar()
        {
            return (char)ReadInt16();
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }

        public unsafe double ReadDouble()
        {
            long num = ReadInt64();
            return *(double*)(&num);
        }

        public unsafe float ReadFloat()
        {
            CheckLength(4);
            int num = ReadInt32();
            return *(float*)(&num);
        }

        public short ReadInt16()
        {
            CheckLength(2);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out short result))
            {
                _ = Read(_cacheBlock, 0, 2);
                result = BitConverter.ToInt16(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapInt16(result);
            return result;
        }

        public int ReadInt32()
        {
            CheckLength(4);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out int result))
            {
                _ = Read(_cacheBlock, 0, 4);
                result = BitConverter.ToInt32(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapInt32(result);
            return result;
        }

        public long ReadInt64()
        {
            CheckLength(8);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out long result))
            {
                _ = Read(_cacheBlock, 0, 8);
                result = BitConverter.ToInt64(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapInt64(result);
            return result;
        }

        public string ReadString(int length)
        {
            CheckLength(length);
            if (length == 0)
                return string.Empty;
            IBuffer rbuffer;

            Span<byte> data;
            Span<char> charSpan = _charCacheBlock.AsSpan();

            if (length < _cacheBlockLength)
            {
                rbuffer = GetAndVerifyReadBuffer();
                int freelen = rbuffer.Length - rbuffer.Postion;
                if (freelen > length)
                {
                    data = rbuffer.Read(length);
                    var l = Decoder.GetChars(data, charSpan, false);
                    return new string(charSpan.Slice(0, l));
                }
            }
            StringBuilder sb = new StringBuilder();
            int rlen;
            while (length > 0)
            {
                rlen = Math.Min(length, _cacheBlockLength);
                rbuffer = GetAndVerifyReadBuffer();
                int freelen = rbuffer.Length - rbuffer.Postion;
                data = rbuffer.Read(Math.Min(freelen, rlen));
                length -= data.Length;
                var l = Decoder.GetChars(data, charSpan, false);
                if (l > 0)
                {
                    sb.Append(charSpan.Slice(0, l));
                }
            }
            return sb.ToString();
        }

        public string ReadString(long length)
        {
            CheckLength(length);
            if (length == 0)
                return string.Empty;
            IBuffer rbuffer;

            Span<byte> data;
            Span<char> charSpan = _charCacheBlock.AsSpan();

            if (length < _cacheBlockLength)
            {
                rbuffer = GetAndVerifyReadBuffer();
                int freelen = rbuffer.Length - rbuffer.Postion;
                if (freelen > length)
                {
                    data = rbuffer.Read((int)length);
                    var l = Decoder.GetChars(data, charSpan, false);
                    return new string(charSpan.Slice(0, l));
                }
            }
            StringBuilder sb = new StringBuilder();
            int rlen;
            while (length > 0)
            {
                if (length > _cacheBlockLength)
                    rlen = _cacheBlockLength;
                else
                    rlen = (int)length;
                rbuffer = GetAndVerifyReadBuffer();
                int freelen = rbuffer.Length - rbuffer.Postion;
                data = rbuffer.Read(Math.Min(freelen, rlen));
                length -= data.Length;
                var l = Decoder.GetChars(data, charSpan, false);
                if (l > 0)
                {
                    sb.Append(charSpan.Slice(0, l));
                }
            }
            return sb.ToString();
        }

        public string ReadToEnd()
        {
            return ReadString(_length);
        }

        public ushort ReadUInt16()
        {
            CheckLength(2);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out ushort result))
            {
                _ = Read(_cacheBlock, 0, 2);
                result = BitConverter.ToUInt16(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapUInt16(result);
            return result;
        }

        public uint ReadUInt32()
        {
            CheckLength(4);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out uint result))
            {
                _ = Read(_cacheBlock, 0, 4);
                result = BitConverter.ToUInt32(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapUInt32(result);
            return result;
        }

        public ulong ReadUInt64()
        {
            CheckLength(8);
            IBuffer rbuffer = GetAndVerifyReadBuffer();
            if (!rbuffer.TryRead(out ulong result))
            {
                _ = Read(_cacheBlock, 0, 8);
                result = BitConverter.ToUInt64(_cacheBlock, 0);
            }
            if (!LittleEndian)
                result = BitHelper.SwapUInt64(result);
            return result;
        }

        public bool TryRead(int count)
        {
            return _length >= count;
        }

        public bool TryReadWith(string eof, out string value, bool returnEof = false)
        {
            return TryReadWith(Encoding.GetBytes(eof), out value, returnEof);
        }

        public bool TryReadWith(byte[] eof, out string value, bool returnEof = false)
        {
            value = null;
            IndexOfResult result = IndexOf(eof);
            int length = result.Length;
            if (result.End != null)
            {
                if (result.Start.Id == result.End.Id)
                {
                    char[] charSpan = _charCacheBlock;
                    if (result.Length < _cacheBlockLength)
                    {
                        var len = Encoding.GetChars(result.Start.Data, result.StartPostion, length, charSpan, 0);
                        if (returnEof)
                            value = new string(charSpan, 0, len);
                        else
                            value = new string(charSpan, 0, len - eof.Length);
                        ReadFree(length);
                    }
                    else
                    {
                        if (returnEof)
                        {
                            value = ReadString(result.Length);
                        }
                        else
                        {
                            value = ReadString(result.Length - eof.Length);
                            Read(eof, 0, eof.Length);
                        }
                    }
                }
                else
                {
                    if (returnEof)
                    {
                        value = ReadString(result.Length);
                    }
                    else
                    {
                        value = ReadString(result.Length - eof.Length);
                        Read(eof, 0, eof.Length);
                    }
                }

                return true;
            }
            return false;
        }

        public bool TryReadWith(byte[] eof, out byte[] value, bool returnEof = false)
        {
            value = null;
            IndexOfResult result = IndexOf(eof);
            if (result.End != null)
            {
                int readLength;
                if (returnEof)
                {
                    readLength = result.Length;
                    value = Read(readLength);
                }
                else
                {
                    readLength = result.Length - eof.Length;
                    value = Read(readLength);
                    Read(eof, 0, eof.Length);
                }
                return true;
            }
            return false;
        }

        private void ReadFree(int count)
        {
            int free = 0;
            IBuffer sbuffer = GetReadBuffer();
            while (sbuffer != null)
            {
                int rc = sbuffer.ReadFree(count);
                free += rc;
                count -= rc;
                if (count == 0)
                    break;
                sbuffer = GetReadBuffer();
            }
        }

        private IndexOfResult IndexOf(byte[] eof)
        {
            IndexOfResult result = new IndexOfResult
            {
                EofData = eof
            };
            if (eof == null || _length < eof.Length)
                return result;
            IBuffer rbuffer = GetReadBuffer();
            if (rbuffer == null)
                return result;
            result.Start = rbuffer;
            result.StartPostion = rbuffer.Postion;
            if (IndexOf(ref result, rbuffer))
            {
                return result;
            }
            rbuffer = rbuffer.Next;
            while (rbuffer != null)
            {
                if (IndexOf(ref result, rbuffer))
                {
                    return result;
                }
                rbuffer = rbuffer.Next;
            }
            return result;
        }

        private bool IndexOf(ref IndexOfResult result, IBuffer buffer)
        {
            int start = buffer.Postion;
            int length = buffer.Length;
            Span<byte> data = buffer.Memory.Span;
            Span<byte> eof = result.EofData;
            int eoflen = eof.Length;
            result.End = buffer;
            int eofindex = result.EofIndex;
            int endpoint;
            for (int i = start; i < length; i++)
            {
                result.Length++;
                if (data[i] == eof[eofindex])
                {
                    eofindex++;
                    endpoint = i;
                    if (eofindex == eoflen)
                    {
                        result.EndPostion = endpoint;
                        return true;
                    }
                }
                else
                {
                    eofindex = 0;
                }
            }
            result.EofIndex = eofindex;
            result.End = null;
            return false;
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Length > 0)
            {
                _readCompletionSource = null;
                var len = Read(buffer, offset, count);
                return Task.FromResult(len);
            }
            else
            {
                _readCompletionSource = new ReadTaskCompletionSource
                {
                    Buffer = buffer,
                    Offset = offset,
                    Count = count
                };
                cancellationToken.Register(() =>
                {
                    _readCompletionSource.TrySetCanceled(cancellationToken);
                    _readCompletionSource = null;
                });
                return _readCompletionSource.Task;
            }
        }

        public byte[] Read(int count)
        {
            CheckLength(count);
            byte[] data = new byte[count];
            Read(data, 0, count);
            return data;
        }

        public ReadResult ReadResult()
        {
            return ReadResult((int)Length);
        }

        public ReadResult ReadResult(int count)
        {
            CheckLength(count);
            int recount = 0;
            int offset = 0;
            IBuffer sbuffer = GetReadBuffer();
            List<Memory<byte>> list = new List<Memory<byte>>();
            while (sbuffer != null)
            {
                var memroy = sbuffer.GetReadMemory(count);
                list.Add(memroy);
                int rc = memroy.Length;
                offset += rc;
                count -= rc;
                recount += rc;
                if (count == 0)
                    break;
                sbuffer = GetReadBuffer();
            }
            return new ReadResult(list);
        }

        private class ReadTaskCompletionSource : TaskCompletionSource<int>
        {
            public byte[] Buffer { get; set; }

            public int Offset { get; set; }

            public int Count { get; set; }
        }

        private void CheckLength(int count)
        {
            if (count > _length)
            {
                ThrowHelper.ThrowDataLessThanReadException();
            }
        }

        private void CheckLength(long count)
        {
            if (count > _length)
            {
                ThrowHelper.ThrowDataLessThanReadException();
            }
        }

        protected override void DisposeCode()
        {
            UpdateReadBuffer();
            if (_readerBuffer != null)
            {
                _readerBuffer.Free();
            }
            if (_readerLastBuffer != null)
            {
                _readerLastBuffer.Free();
            }
        }
    }
}