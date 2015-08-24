using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Utilities
{
    public static class EnumerableUtility
    {
        public static IEnumerator<T> GetEmptyEnumerator<T>()
        {
            return EmptyEnumerator<T>.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<T> GetEmptyEnumerable<T>()
        {
            return EmptyEnumerable<T>.Instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EmptyEnumerable<T> : IEnumerable<T>
        {
            #region Implementation of IEnumerable

            public IEnumerator<T> GetEnumerator()
            {
                return EmptyEnumerator<T>.Instance;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<T>.Instance;
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static readonly EmptyEnumerable<T> Instance = new EmptyEnumerable<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EmptyEnumerator<T> : IEnumerator<T>
        {
            #region Implementation of IDisposable

            public void Dispose()
            {
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T Current
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object IEnumerator.Current
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static readonly EmptyEnumerator<T> Instance = new EmptyEnumerator<T>();
        }
    }
}
