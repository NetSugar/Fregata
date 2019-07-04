using Fregata.Buffers;
using Fregata.Framing;
using Fregata.Options;
using Xunit;

namespace Fregata.Test.Framing
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/27 15:55:28
    /// </summary>
    public class LengthPrefixMessageFramerTest
    {
        private readonly FregataOptions _fregataOptions;

        public LengthPrefixMessageFramerTest()
        {
            _fregataOptions = new FregataOptions();
        }

        [Fact(DisplayName = "Test buffer package and unpackage")]
        public void Test()
        {
            IMessageFramer messageFramer = new LengthPrefixMessageFramer(_fregataOptions);
            var bufferPipelinse = new BufferPipeline();
            for (int i = 0; i < 10000; i++)
            {
                messageFramer.FrameData(bufferPipelinse.Writer, new byte[4] { 1, 2, 3, 4 });
            }
            bufferPipelinse.Writer.Flush();
            messageFramer.RegisterMessageArrivedCallback((result) =>
            {
                Assert.True(result.Length == 4);
                for (int i = 0; i < result.Length; i++)
                {
                    Assert.True(result.Buffer.Span[i] == i + 1);
                }
            });
            messageFramer.UnFrameData(bufferPipelinse.Reader);
        }
    }
}