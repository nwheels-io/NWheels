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
                return string.Empty;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public struct LogNameValuePair<T>
    {
        public string Name;
        public T Value;
        public bool IsDetail;
        public string Format;

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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1)
            : base(messageId, level, exception)
        {
            _value1 = value1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return
                MessageIdToText() + 
                (_value1.IsDetail ? "" : ", " + FormatNameValuePair(_value1.Name, _value1.FormatValue()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return
                (_value1.IsDetail ? FormatNameValuePair(_value1.Name, _value1.FormatValue()) + System.Environment.NewLine : "") +
                base.FormatFullDetailsText();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return (
                base.FormatNameValuePairsText(delimiter) + delimiter +
                FormatNameValuePair(_value1.Name, _value1.FormatValue()));
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return
                MessageIdToText() +
                (_value1.IsDetail ? "" : ", " + FormatNameValuePair(_value1.Name, _value1.FormatValue())) +
                (_value2.IsDetail ? "" : ", " + FormatNameValuePair(_value2.Name, _value2.FormatValue()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return
                (_value1.IsDetail ? FormatNameValuePair(_value1.Name, _value1.FormatValue()) + System.Environment.NewLine : "") +
                (_value2.IsDetail ? FormatNameValuePair(_value2.Name, _value2.FormatValue()) + System.Environment.NewLine : "") +
                base.FormatFullDetailsText();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                FormatNameValuePair(_value1.Name, _value1.FormatValue()) + delimiter +
                FormatNameValuePair(_value2.Name, _value2.FormatValue());
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3)
            : base(messageId, level, exception)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return
                MessageIdToText() +
                (_value1.IsDetail ? "" : ", " + FormatNameValuePair(_value1.Name, _value1.FormatValue())) +
                (_value2.IsDetail ? "" : ", " + FormatNameValuePair(_value2.Name, _value2.FormatValue())) +
                (_value3.IsDetail ? "" : ", " + FormatNameValuePair(_value3.Name, _value3.FormatValue()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return
                (_value1.IsDetail ? FormatNameValuePair(_value1.Name, _value1.FormatValue()) + System.Environment.NewLine : "") +
                (_value2.IsDetail ? FormatNameValuePair(_value2.Name, _value2.FormatValue()) + System.Environment.NewLine : "") +
                (_value3.IsDetail ? FormatNameValuePair(_value3.Name, _value3.FormatValue()) + System.Environment.NewLine : "") +
                base.FormatFullDetailsText();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return
                base.FormatNameValuePairsText(delimiter) + delimiter +
                FormatNameValuePair(_value1.Name, _value1.FormatValue()) + delimiter +
                FormatNameValuePair(_value2.Name, _value2.FormatValue()) + delimiter +
                FormatNameValuePair(_value3.Name, _value3.FormatValue());
        }
    }
}
