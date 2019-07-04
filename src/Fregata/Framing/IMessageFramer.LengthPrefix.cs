using Fregata.Buffers;
using Fregata.Options;
using Fregata.Utils;
using System;

namespace Fregata.Framing
{
    public class LengthPrefixMessageFramer : IMessageFramer
    {
        private readonly int _headerSize;

        private int _packageLength = -1;
        private Action<ReadResult> _receivedHandler;

        public LengthPrefixMessageFramer(FregataOptions fregataOptions)
        {
            _headerSize = fregataOptions.LengthField;
        }

        public void FrameData(IBufferWriter bufferWriter, ReadOnlyMemory<byte> data)
        {
            WriterHeader(bufferWriter, data.Length);
            bufferWriter.Write(data);
        }

        private void WriterHeader(IBufferWriter bufferWriter, int length)
        {
            switch (_headerSize)
            {
                case 1:
                    bufferWriter.Write((byte)length);
                    break;

                case 2:
                    bufferWriter.Write((short)length);
                    break;

                case 4:
                    bufferWriter.Write(length);
                    break;

                default:
                    ThrowHelper.ThrowLengthFieldConfigErrorException();
                    break;
            }
        }

        public void RegisterMessageArrivedCallback(Action<ReadResult> handler)
        {
            _receivedHandler = handler ?? throw new ArgumentNullException("handler");
        }

        public void UnFrameData(IBufferReader readerBuffer)
        {
            if (_packageLength == -1)
            {
                if (readerBuffer.TryRead(_headerSize))
                {
                    ReadHeader(readerBuffer);
                    ReadData(readerBuffer);
                }
            }
            else
            {
                ReadData(readerBuffer);
            }
        }

        private void ReadData(IBufferReader readerBuffer)
        {
            if (readerBuffer.TryRead(_packageLength))
            {
                var data = readerBuffer.ReadResult(_packageLength);
                if (_receivedHandler != null)
                {
                    try
                    {
                        _receivedHandler(data);
                    }
                    catch (Exception ex)
                    {
                        Log<LengthPrefixMessageFramer>.Error(ex, "Handle received message fail.");
                    }
                }
                _packageLength = -1;
            }
        }

        public bool CanUnFrameData(IBufferReader readerBuffer)
        {
            if (_packageLength == -1)
            {
                return readerBuffer.TryRead(_headerSize);
            }
            else
            {
                return readerBuffer.TryRead(_packageLength);
            }
        }

        private void ReadHeader(IBufferReader readerBuffer)
        {
            switch (_headerSize)
            {
                case 1:
                    _packageLength = readerBuffer.ReadByte();
                    break;

                case 2:
                    _packageLength = readerBuffer.ReadInt16();
                    break;

                case 4:
                    _packageLength = readerBuffer.ReadInt32();
                    break;

                default:
                    ThrowHelper.ThrowLengthFieldConfigErrorException();
                    break;
            }
        }
    }
}