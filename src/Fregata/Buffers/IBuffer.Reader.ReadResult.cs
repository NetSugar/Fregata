using System;
using System.Collections.Generic;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/27 11:08:03
    /// </summary>
    public struct ReadResult
    {
        private readonly Memory<byte> memory;

        public ReadResult(IList<Memory<byte>> blocks)
        {
            memory = blocks.Combine();
        }

        public ReadOnlyMemory<byte> Buffer
        {
            get
            {
                return memory;
            }
        }

        public int Length
        {
            get
            {
                return memory.Length;
            }
        }
    }
}