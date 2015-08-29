using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hapil;

namespace NWheels.Concurrency
{
    public abstract class ContextAnchor<T>
    {
        public abstract void Clear();
        public abstract T Current { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ThreadStaticAnchor<T> : ContextAnchor<T>
    {
        [ThreadStatic]
        private static T _s_current;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ContextAnchor<T>

        public override void Clear()
        {
            Current = default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override T Current
        {
            get { return _s_current; }
            set { _s_current = value; }
        }

        #endregion
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class LogicalCallContextAnchor<T> : ContextAnchor<T>
    {
        private static readonly string _s_logicalCallContextKey = typeof(LogicalCallContextAnchor<T>).FriendlyName();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ContextAnchor<T>

        public override void Clear()
        {
            Current = default(T);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override T Current
        {
            get
            {
                return (T)CallContext.LogicalGetData(_s_logicalCallContextKey);
            }
            set
            {
                CallContext.LogicalSetData(_s_logicalCallContextKey, value);
            }
        }

        #endregion
    }
}
