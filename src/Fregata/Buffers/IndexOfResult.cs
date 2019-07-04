namespace Fregata.Buffers
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/12 15:56:53
    /// </summary>
    public struct IndexOfResult
    {
        public IMemoryBlock Start;

        public int StartPostion;

        public IMemoryBlock End;

        public int EndPostion;

        public int Length;

        public byte[] EofData;

        public int EofIndex;
    }
}