namespace Fregata.Exceptions
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/27 18:51:01
    /// </summary>
    public class ServerNameNotBeNullException : FregataException
    {
        public ServerNameNotBeNullException() : base("server socket name can not be null.")
        {
        }
    }
}