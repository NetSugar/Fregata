using System;
using System.Text;

namespace Fregata.Exceptions
{
    internal static class ExceptionExtension
    {
        public static string ToErrMsg(this Exception ex, string desc = null)
        {
            if (ex == null) return "";
            StringBuilder errorBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(desc))
            {
                errorBuilder.AppendFormat("desc：{0}", desc).AppendLine();
            }
            errorBuilder.AppendFormat("Message：{0}", ex.Message).AppendLine();
            if (ex.InnerException != null)
            {
                if (!string.Equals(ex.Message, ex.InnerException.Message, StringComparison.OrdinalIgnoreCase))
                {
                    errorBuilder.AppendFormat("InnerException：{0}", ex.InnerException.Message).AppendLine();
                }
            }
            errorBuilder.AppendFormat("Source：{0}", ex.Source).AppendLine();
            errorBuilder.AppendFormat("StackTrace：{0}", ex.StackTrace).AppendLine();
            return errorBuilder.ToString();
        }
    }
}