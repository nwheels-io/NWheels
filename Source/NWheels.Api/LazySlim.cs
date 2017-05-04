using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels
{
    public struct LazySlim<T>
    {
        private readonly Func<T> _factory;
        private bool _hasValue;
        private T _value;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public LazySlim(T value)
        {
            _value = value;
            _factory = null;
            _hasValue = true;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public LazySlim(Func<T> factory)
        {
            _value = default(T);
            _factory = factory;
            _hasValue = false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    _value = _factory();
                    _hasValue = true;
                }

                return _value;
            }
        }
    }
}
