using Fregata.Buffers;
using System;
using Xunit;

namespace Fregata.Test.Buffers
{
    public class BufferPipelineTest
    {
        [Fact(DisplayName = "buffer pipe write read test")]
        public void Int()
        {
            var pipeline = new BufferPipeline(new BufferPoolCreater().Create(2, 100));
            var reader = pipeline.Reader;
            var writer = pipeline.Writer;
            writer.Write(true);
            writer.Write((short)1);
            writer.Write(32);
            writer.Write((long)9999999999);
            writer.Write((ushort)2);
            writer.Write((uint)2);
            writer.Write((ulong)3);
            var dateTime = DateTime.Now;
            writer.Write(dateTime);
            var str = "adssadasdasd萨哈克的哈萨克等你们";
            writer.Write(str);
            writer.Flush();
            reader.Read(writer, (int)reader.Length);
            writer.Flush();
            Assert.True(reader.ReadBool());
            Assert.True(reader.ReadInt16() == 1);
            Assert.True(reader.ReadInt32() == 32);
            Assert.True(reader.ReadInt64() == 9999999999);
            Assert.True(reader.ReadUInt16() == 2);
            Assert.True(reader.ReadUInt32() == 2);
            Assert.True(reader.ReadUInt64() == 3);
            Assert.True(reader.ReadDateTime() == dateTime);
            Assert.True(str == reader.ReadToEnd());
        }
    }
}