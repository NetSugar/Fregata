using System.Text;

namespace Fregata.Options
{
    public class FregataOptions
    {
        /// <summary>
        /// must be either 2, 4
        /// </summary>
        public int LengthField { get; set; } = 4;

        public bool LittleEndian { get; set; } = true;
        public string EncodeName { get; set; } = "utf-8";

        public Encoding Encode
        {
            get
            {
                return Encoding.GetEncoding(EncodeName);
            }
        }

        public int SendBufferSize { get; set; } = 1024;
        public int SendBufferInitialCount { get; set; } = 1024 * 4;
        public int SendMaxPacketSize = 1024 * 64;

        public int ReceiveBufferSize { get; set; } = 1024;
        public int ReceiveBufferInitialCount { get; set; } = 1024 * 4;
        public int SendSocketAsyncEventArgsPoolMinCount { get; set; } = 50;

        public int SendMaxSize = 1024 * 64;
        public int ReceiveMaxSize = 1024 * 64;

        public int ServerMaxPendingConnections { get; set; } = 5000;

        public int ClientConnectionWaitMilliseconds { get; set; } = 5000;
    }
}