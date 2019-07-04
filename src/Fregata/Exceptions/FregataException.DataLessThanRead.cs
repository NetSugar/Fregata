namespace Fregata.Exceptions
{
    internal class DataLessThanReadException : FregataException
    {
        public DataLessThanReadException() : base("data length less than the read count.")
        {
        }
    }
}