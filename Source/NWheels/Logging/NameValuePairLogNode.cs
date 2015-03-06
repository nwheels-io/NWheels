using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Logging
{
    public class NameValuePairLogNode : LogNode
    {
        private readonly Exception _exception;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception)
            : base(messageId, LogContentTypes.Text, level)
        {
            _exception = exception;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            if ( _exception != null )
            {
                return _exception.ToString();
            }
            else
            {
                return null;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct LogNameValuePair<T>
    {
        public string Name;
        public T Value;
        public bool IsDetail;
        public string Format;
        public LogContentTypes ContentType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Pure]
        public string FormatValue()
        {
            var formattable = Value as IFormattable;

            if ( Format != null && formattable != null )
            {
                return formattable.ToString(Format, CultureInfo.CurrentCulture);
            }
            else if ( !typeof(T).IsValueType && (object)Value == null )
            {
                return "null";
            }
            else
            {
                return Value.ToString();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FormatLogString()
        {
            return LogNode.FormatNameValuePair(this.Name, this.FormatValue());
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            base.BubbleContentTypesFrom(_value1.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            
            base.BubbleContentTypesFrom(_value1.ContentType | _value2.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;

            base.BubbleContentTypesFrom(_value1.ContentType | _value2.ContentType | _value3.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return 
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;

            base.BubbleContentTypesFrom(_value1.ContentType | _value2.ContentType | _value3.ContentType | _value4.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended)
                .AppendToFullDetailsText(ref _value4, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;

            base.BubbleContentTypesFrom(
                _value1.ContentType | _value2.ContentType | _value3.ContentType | _value4.ContentType | 
                _value5.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended)
                .AppendToFullDetailsText(ref _value4, ref anyAppended)
                .AppendToFullDetailsText(ref _value5, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;

            base.BubbleContentTypesFrom(
                _value1.ContentType | _value2.ContentType | _value3.ContentType | _value4.ContentType | 
                _value5.ContentType | _value6.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended)
                .AppendToFullDetailsText(ref _value4, ref anyAppended)
                .AppendToFullDetailsText(ref _value5, ref anyAppended)
                .AppendToFullDetailsText(ref _value6, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;
        private LogNameValuePair<T7> _value7;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;

            base.BubbleContentTypesFrom(
                _value1.ContentType | _value2.ContentType | _value3.ContentType | _value4.ContentType | 
                _value5.ContentType | _value6.ContentType | _value7.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended)
                .AppendToSingleLineText(ref _value7, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended)
                .AppendToFullDetailsText(ref _value4, ref anyAppended)
                .AppendToFullDetailsText(ref _value5, ref anyAppended)
                .AppendToFullDetailsText(ref _value6, ref anyAppended)
                .AppendToFullDetailsText(ref _value7, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString() + delimiter +
                _value7.FormatLogString();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7, T8> : NameValuePairLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;
        private LogNameValuePair<T7> _value7;
        private LogNameValuePair<T8> _value8;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7, LogNameValuePair<T8> value8)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;
            _value8 = value8;

            base.BubbleContentTypesFrom(
                _value1.ContentType | _value2.ContentType | _value3.ContentType | _value4.ContentType |
                _value5.ContentType | _value6.ContentType | _value7.ContentType | _value8.ContentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            var anyAppended = false;

            return
                MessageIdToText()
                .AppendToSingleLineText(ref _value1, ref anyAppended)
                .AppendToSingleLineText(ref _value2, ref anyAppended)
                .AppendToSingleLineText(ref _value3, ref anyAppended)
                .AppendToSingleLineText(ref _value4, ref anyAppended)
                .AppendToSingleLineText(ref _value5, ref anyAppended)
                .AppendToSingleLineText(ref _value6, ref anyAppended)
                .AppendToSingleLineText(ref _value7, ref anyAppended)
                .AppendToSingleLineText(ref _value8, ref anyAppended);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            bool anyAppended = false;

            return (
                string.Empty
                .AppendToFullDetailsText(ref _value1, ref anyAppended)
                .AppendToFullDetailsText(ref _value2, ref anyAppended)
                .AppendToFullDetailsText(ref _value3, ref anyAppended)
                .AppendToFullDetailsText(ref _value4, ref anyAppended)
                .AppendToFullDetailsText(ref _value5, ref anyAppended)
                .AppendToFullDetailsText(ref _value6, ref anyAppended)
                .AppendToFullDetailsText(ref _value7, ref anyAppended)
                .AppendToFullDetailsText(ref _value8, ref anyAppended) +
                base.FormatFullDetailsText())
                .NullIfEmptyOrWhitespace();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                _value1.FormatLogString() + delimiter +
                _value2.FormatLogString() + delimiter +
                _value3.FormatLogString() + delimiter +
                _value4.FormatLogString() + delimiter +
                _value5.FormatLogString() + delimiter +
                _value6.FormatLogString() + delimiter +
                _value7.FormatLogString() + delimiter +
                _value8.FormatLogString();
        }
    }
}
