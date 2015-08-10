using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public class DelegatingTransformingEnumerator<TIn, TOut> : TransformingEnumerator<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _transform;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DelegatingTransformingEnumerator(IEnumerator<TIn> underlyingEnumerator, Func<TIn, TOut> transform)
            : base(underlyingEnumerator)
        {
            _transform = transform;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TOut Transform(TIn input)
        {
            return _transform(input);
        }
    }
}
