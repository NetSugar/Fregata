using System;

namespace Fregata.Utils
{
    public static class ExceptionUtil
    {
        public static void Eat(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
            }
        }

        public static T Eat<T>(Func<T> action, T defaultValue = default)
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}