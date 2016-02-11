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
        public NameValuePairActivityLogNode(string messageId, LogLevel level, LogOptions options)
            : base(messageId, level, options)
        {
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override string FormatFullDetailsText()
        {
            return
                base.FormatFullDetailsText() +
                (base.Exception != null ? Environment.NewLine + base.Exception.ToString() : string.Empty);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(string messageId, LogLevel level, LogOptions options, LogNameValuePair<T1> value1)
            : base(messageId, level, options)
        {
            Value1 = value1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1  
            });
        }
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(string messageId, LogLevel level, LogOptions options, LogNameValuePair<T1> value1, LogNameValuePair<T2> value2)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3, T4> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;
        public LogNameValuePair<T4> Value4;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            AppendGroupKeyUp(key, ref Value4, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3,
                Value4
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;
        public LogNameValuePair<T4> Value4;
        public LogNameValuePair<T5> Value5;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            AppendGroupKeyUp(key, ref Value4, ref first);
            AppendGroupKeyUp(key, ref Value5, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3,
                Value4,
                Value5
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;
        public LogNameValuePair<T4> Value4;
        public LogNameValuePair<T5> Value5;
        public LogNameValuePair<T6> Value6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, 
            LogNameValuePair<T6> value6)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            AppendGroupKeyUp(key, ref Value4, ref first);
            AppendGroupKeyUp(key, ref Value5, ref first);
            AppendGroupKeyUp(key, ref Value6, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3,
                Value4,
                Value5,
                Value6
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6, T7> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;
        public LogNameValuePair<T4> Value4;
        public LogNameValuePair<T5> Value5;
        public LogNameValuePair<T6> Value6;
        public LogNameValuePair<T7> Value7;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, 
            LogNameValuePair<T6> value6, LogNameValuePair<T7> value7)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
            Value7 = value7;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            AppendGroupKeyUp(key, ref Value4, ref first);
            AppendGroupKeyUp(key, ref Value5, ref first);
            AppendGroupKeyUp(key, ref Value6, ref first);
            AppendGroupKeyUp(key, ref Value7, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3,
                Value4,
                Value5,
                Value6,
                Value7
            });
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class NameValuePairActivityLogNode<T1, T2, T3, T4, T5, T6, T7, T8> : NameValuePairActivityLogNode
    {
        public LogNameValuePair<T1> Value1;
        public LogNameValuePair<T2> Value2;
        public LogNameValuePair<T3> Value3;
        public LogNameValuePair<T4> Value4;
        public LogNameValuePair<T5> Value5;
        public LogNameValuePair<T6> Value6;
        public LogNameValuePair<T7> Value7;
        public LogNameValuePair<T8> Value8;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NameValuePairActivityLogNode(
            string messageId, LogLevel level, LogOptions options, 
            LogNameValuePair<T1> value1, LogNameValuePair<T2> value2, LogNameValuePair<T3> value3, LogNameValuePair<T4> value4, LogNameValuePair<T5> value5, 
            LogNameValuePair<T6> value6, LogNameValuePair<T7> value7, LogNameValuePair<T8> value8)
            : base(messageId, level, options)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
            Value5 = value5;
            Value6 = value6;
            Value7 = value7;
            Value8 = value8;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ActivityLogNode

        public override string GetStatsGroupKey()
        {
            bool first = true;
            var key = new StringBuilder(base.GetStatsGroupKey());
            AppendGroupKeyUp(key, ref Value1, ref first);
            AppendGroupKeyUp(key, ref Value2, ref first);
            AppendGroupKeyUp(key, ref Value3, ref first);
            AppendGroupKeyUp(key, ref Value4, ref first);
            AppendGroupKeyUp(key, ref Value5, ref first);
            AppendGroupKeyUp(key, ref Value6, ref first);
            AppendGroupKeyUp(key, ref Value7, ref first);
            AppendGroupKeyUp(key, ref Value8, ref first);
            return EndGroupKey(key, ref first);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            return base.ListNameValuePairs().Concat(new ILogNameValuePair[] {
                Value1,
                Value2,
                Value3,
                Value4,
                Value5,
                Value6,
                Value7,
                Value8
            });
        }
    }
}
