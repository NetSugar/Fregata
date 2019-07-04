using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fregata.Buffers
{
    public static class BufferUtil
    {
        public static ArraySegment<byte> GetArray(this Memory<byte> memory)
        {
            return GetArray((ReadOnlyMemory<byte>)memory);
        }

        public static ArraySegment<byte> GetArray(this ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }

        public static Memory<byte> Combine(this IList<Memory<byte>> memorys)
        {
            int length = memorys.Sum(m => m.Length);
            var memory = new Memory<byte>(new byte[length]);
            int position = 0;
            foreach (var item in memorys)
            {
                item.CopyTo(memory.Slice(position, item.Length));
            }
            return memory;
        }
    }
}