using Fregata.Buffers;
using Fregata.Framing;
using Fregata.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpanMemoryTest
{
    internal class Program
    {
        private static int _dealCount = 0;
        private static void Main(string[] args)
        {
            Test3();
            Console.ReadLine();
        }

        public static void Test1()
        {
            Console.WriteLine("memory cpu test.");
            IMessageFramer messageFramer = new LengthPrefixMessageFramer(new FregataOptions());
            var bufferPipelinse = new BufferPipeline();
            for (int i = 0; i < 500000; i++)
            {
                messageFramer.FrameData(bufferPipelinse.Writer, new byte[4] { 1, 2, 3, 4 });
            }
            bufferPipelinse.Writer.Flush();
            messageFramer.RegisterMessageArrivedCallback((result) =>
            {
                _dealCount++;
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        Console.Write(result.Buffer.Span[i]);
                    }
                    Console.WriteLine();
                });
            });
            while (messageFramer.CanUnFrameData(bufferPipelinse.Reader))
            {
                messageFramer.UnFrameData(bufferPipelinse.Reader);
            }
            Console.WriteLine("================");
            Console.WriteLine(_dealCount);
        }

        public static void Test2()
        {
            Console.WriteLine("test when memory changed the new data will not be change");
            Memory<byte> memory = new byte[2] { 0, 2 };
            var data = new byte[1];
            data[0] = memory.Span[0];
            memory.Span[0] = 1;
            Console.WriteLine(data[0]);
            Console.WriteLine(memory.Span[0]);
            Console.WriteLine("test when memory changed the new memory will not be change");
            List<Memory<byte>> list = new List<Memory<byte>>
            {
                memory
            };
            var newMemory = list.Combine();
            Console.Write(newMemory.Span[0]);
            Console.WriteLine(newMemory.Span[1]);

            Console.Write(memory.Span[0]);
            Console.WriteLine(memory.Span[1]);
            memory.Span[0] = 0;
            Console.Write(newMemory.Span[0]);
            Console.WriteLine(newMemory.Span[1]);

            Console.Write(memory.Span[0]);
            Console.WriteLine(memory.Span[1]);
            Console.WriteLine("test read result size");
            _ = new ReadResult(list);
            var list2 = new List<Memory<byte>>
            {
                memory,
                memory,
                memory,
                newMemory
            };
            _ = new ReadResult(list2);
        }

        public static void Test3()
        {
            var data = new byte[2] { 1, 2 };
            var memory = new Memory<byte>(data);
            data[0] = 2;
            Console.WriteLine(memory.Span[0]);
        }
    }
}