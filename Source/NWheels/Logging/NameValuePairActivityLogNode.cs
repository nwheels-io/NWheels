using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Logging
{
    public class NameValuePairActivityLogNode : ActivityLogNode
    {
        public NameValuePairActivityLogNode(string messageId)
            : base(messageId)
        {
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatSingleLineText()
        {
            return base.FormatSingleLineText();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            if ( base.Exception != null )
            {
                return base.FormatFullDetailsText() + System.Environment.NewLine + base.Exception.ToString();
            }
            else
            {
                return base.FormatFullDetailsText().NullIfEmptyOrWhitespace();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatNameValuePairsText(string delimiter)
        {
            return base.FormatNameValuePairsText(delimiter);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(string messageId, LogNameValuePair<T1> value1)
            : base(messageId)
        {
            _value1 = value1;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(string messageId, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2) 
                .AppendToFullDetailsText(ref _value3) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3, T4> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6, T7> : NameValuePairActivityLogNode
    {
        private LogNameValuePair<T1> _value1;
        private LogNameValuePair<T2> _value2;
        private LogNameValuePair<T3> _value3;
        private LogNameValuePair<T4> _value4;
        private LogNameValuePair<T5> _value5;
        private LogNameValuePair<T6> _value6;
        private LogNameValuePair<T7> _value7;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6)
                .AppendToFullDetailsText(ref _value7) +
                base.FormatFullDetailsText()
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

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6, T7, T8> : NameValuePairActivityLogNode
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

        public NameValuePairActivityLogNode(
            string messageId,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7, LogNameValuePair<T8> value8)
            : base(messageId)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
            _value7 = value7;
            _value8 = value8;
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
            return string.Empty
                .AppendToFullDetailsText(ref _value1)
                .AppendToFullDetailsText(ref _value2)
                .AppendToFullDetailsText(ref _value3)
                .AppendToFullDetailsText(ref _value4)
                .AppendToFullDetailsText(ref _value5)
                .AppendToFullDetailsText(ref _value6)
                .AppendToFullDetailsText(ref _value7)
                .AppendToFullDetailsText(ref _value8) +
                base.FormatFullDetailsText()
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
