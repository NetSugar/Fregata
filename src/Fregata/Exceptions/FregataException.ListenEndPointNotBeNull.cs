namespace Fregata.Exceptions
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/6/27 18:54:00
    /// </summary>
    public class ListenEndPointNotBeNullException : FregataException
    {
        public ListenEndPointNotBeNullException() : base("server listening endpoint not be null.")
        {
        }
    }
}