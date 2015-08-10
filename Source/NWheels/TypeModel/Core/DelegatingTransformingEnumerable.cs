using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public class DelegatingTransformingEnumerable<TIn, TOut> : IEnumerable<TOut>
    {
        private readonly IEnumerable<TIn> _underlyingEnumerable;
        private readonly Func<TIn, TOut> _transform;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DelegatingTransformingEnumerable(IEnumerable<TIn> underlyingEnumerable, Func<TIn, TOut> transform)
        {
            _transform = transform;
            _underlyingEnumerable = underlyingEnumerable;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TOut> GetEnumerator()
        {
            return new DelegatingTransformingEnumerator<TIn, TOut>(_underlyingEnumerable.GetEnumerator(), _transform);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
