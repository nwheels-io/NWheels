using System;
using System.Collections.Generic;
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

        protected NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogContentTypes contentTypes)
            : base(messageId, contentTypes, level)
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
            return 
                base.FormatFullDetailsText() + 
                (_exception != null ? Environment.NewLine + _exception.ToString() : string.Empty);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1)
            : base(
                messageId, level, exception, 
                value1.ContentTypes)
        {
            _value1 = value1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1  
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(string messageId, LogLevel level, Exception exception, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(
                messageId, level, exception, 
                value1.ContentTypes | value2.ContentTypes)
        {
            _value1 = value1;
            _value2 = value2;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2
            });
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
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;
        private readonly LogNameValuePair<T4> _value4;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4)
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes | value4.ContentTypes)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3,
                _value4
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;
        private readonly LogNameValuePair<T4> _value4;
        private readonly LogNameValuePair<T5> _value5;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5)
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes | value4.ContentTypes | value5.ContentTypes)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3,
                _value4,
                _value5
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;
        private readonly LogNameValuePair<T4> _value4;
        private readonly LogNameValuePair<T5> _value5;
        private readonly LogNameValuePair<T6> _value6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6)
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes | value4.ContentTypes | value5.ContentTypes | value6.ContentTypes)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
            _value4 = value4;
            _value5 = value5;
            _value6 = value6;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3,
                _value4,
                _value5,
                _value6
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;
        private readonly LogNameValuePair<T4> _value4;
        private readonly LogNameValuePair<T5> _value5;
        private readonly LogNameValuePair<T6> _value6;
        private readonly LogNameValuePair<T7> _value7;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7)
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes | value4.ContentTypes | 
                value5.ContentTypes | value6.ContentTypes | value7.ContentTypes)
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

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3,
                _value4,
                _value5,
                _value6,
                _value7
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairLogNode<T1, T2, T3, T4, T5, T6, T7, T8> : NameValuePairLogNode
    {
        private readonly LogNameValuePair<T1> _value1;
        private readonly LogNameValuePair<T2> _value2;
        private readonly LogNameValuePair<T3> _value3;
        private readonly LogNameValuePair<T4> _value4;
        private readonly LogNameValuePair<T5> _value5;
        private readonly LogNameValuePair<T6> _value6;
        private readonly LogNameValuePair<T7> _value7;
        private readonly LogNameValuePair<T8> _value8;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairLogNode(
            string messageId, LogLevel level, Exception exception,
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, LogNameValuePair<T6> value6, LogNameValuePair<T7> value7, LogNameValuePair<T8> value8)
            : base(
                messageId, level, exception,
                value1.ContentTypes | value2.ContentTypes | value3.ContentTypes | value4.ContentTypes | 
                value5.ContentTypes | value6.ContentTypes | value7.ContentTypes | value8.ContentTypes)
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

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                _value1,
                _value2,
                _value3,
                _value4,
                _value5,
                _value6,
                _value7,
                _value8
            });
        }
    }
}
