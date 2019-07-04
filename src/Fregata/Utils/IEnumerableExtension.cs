using System;
using System.Collections.Generic;

namespace Fregata.Utils
{
    /// <summary>
    /// desc：
    /// author：yjq 2019/7/3 17:17:25
    /// </summary>
    public static class IEnumerableExtension
    {
        public static void Excute<T>(this IList<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action.Invoke(item);
            }
        }
    }
}