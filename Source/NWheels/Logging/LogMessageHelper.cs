using NWheels.Extensions;

namespace NWheels.Logging
{
    internal static class LogMessageHelper
    {
        public static string GetTextFromMessageId(string messageId)
        {
            var lastDotPosition = messageId.LastIndexOf('.');

            if ( lastDotPosition >= 0 && lastDotPosition < messageId.Length - 1 )
            {
                return messageId.Substring(lastDotPosition + 1).SplitPascalCase();
            }
            else
            {
                return messageId.SplitPascalCase();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AppendToSingleLineText<T>(this string s, ref LogNameValuePair<T> pair, ref bool anyAppended)
        {
            if ( !pair.IsDetail )
            {
                var result = s + (anyAppended ? ", " : ": ") + pair.FormatLogString();
                anyAppended = true;
                return result;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AppendToFullDetailsText<T>(this string s, ref LogNameValuePair<T> pair, ref bool anyAppended)
        {
            if ( pair.IsDetail )
            {
                var result = 
                    s + 
                        (s == "" || s.EndsWith(System.Environment.NewLine) ? "" : System.Environment.NewLine) + 
                        pair.FormatLogString() + 
                        System.Environment.NewLine;

                anyAppended = true;
                return result;
            }
            else
            {
                return s;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AppendToExceptionMessage<T>(this string s, ref LogNameValuePair<T> pair, ref bool anyAppended)
        {
            var result = s + (anyAppended ? ", " : ": ") + pair.FormatLogString();
            anyAppended = true;
            return result;
        }
    }
}