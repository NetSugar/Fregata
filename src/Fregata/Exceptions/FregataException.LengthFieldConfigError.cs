namespace Fregata.Exceptions
{
    public class LengthFieldConfigErrorException : FregataException
    {
        public LengthFieldConfigErrorException() : base("data package length must be in 1,2,4 .")
        {
        }
    }
}