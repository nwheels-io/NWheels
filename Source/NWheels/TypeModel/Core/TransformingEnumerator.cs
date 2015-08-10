using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public abstract class TransformingEnumerator<TIn, TOut> : IEnumerator<TOut>
    {
        private readonly IEnumerator<TIn> _underlyingEnumerator;
        private TOut _current;
        private bool _hasCurrent;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected TransformingEnumerator(IEnumerator<TIn> underlyingEnumerator/*, Func<TIn, TOut> transform*/)
        {
            _underlyingEnumerator = underlyingEnumerator;
            _hasCurrent = false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _underlyingEnumerator.Dispose();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool MoveNext()
        {
            if (_underlyingEnumerator.MoveNext())
            {
                _current = Transform(_underlyingEnumerator.Current);
                _hasCurrent = true;
            }
            else
            {
                _hasCurrent = false;
            }

            return _hasCurrent;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Reset()
        {
            _underlyingEnumerator.Reset();
            _hasCurrent = false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TOut Current
        {
            get
            {
                if ( _hasCurrent )
                {
                    return _current;
                }
                else
                {
                    throw new InvalidOperationException("Sequence has no more results available.");
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract TOut Transform(TIn input);
    }
}
