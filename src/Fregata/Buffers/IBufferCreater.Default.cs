using System;
using System.Collections.Generic;
using System.Text;

namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/3 10:30:29
    /// </summary>
    internal class BufferCreater : IBufferCreater
    {
        public IBuffer Create(IBufferPool bufferPool, int size)
        {
            return new Buffer(size)
            {
                Pool = bufferPool
            };
        }
    }
}
