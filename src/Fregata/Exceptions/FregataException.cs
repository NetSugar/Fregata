using System;

namespace Fregata.Exceptions
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/5/30 18:18:22
    /// </summary>
    public class FregataException : Exception
    {
        public FregataException()
        {
        }

        public FregataException(string message) : base(message)
        {
        }

        public FregataException(string message, params object[] parameters) : base(string.Format(message, parameters))
        {
        }

        public FregataException(string message, Exception baseError) : base(message, baseError)
        {
        }

        public FregataException(Exception baseError, string message, params object[] parameters) : base(string.Format(message, parameters), baseError)
        {
        }
    }
}