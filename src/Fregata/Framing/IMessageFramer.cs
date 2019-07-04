using Fregata.Buffers;
using System;

namespace Fregata.Framing
{
    public interface IMessageFramer
    {
        void UnFrameData(IBufferReader readerBuffer);

        bool CanUnFrameData(IBufferReader readerBuffer);

        void FrameData(IBufferWriter bufferWriter, ReadOnlyMemory<byte> data);

        void RegisterMessageArrivedCallback(Action<ReadResult> handler);
    }
}