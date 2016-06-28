using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency.Impl
{
    public class WriteOnlyCollectionWrapper<T> : IWriteOnlyCollection<T>
    {
        private readonly ICollection<T> _inner;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WriteOnlyCollectionWrapper(ICollection<T> inner)
        {
            _inner = inner;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IWriteOnlyCollection<in T>

        public void Add(T item)
        {
            _inner.Add(item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Count
        {
            get { return _inner.Count; }
        }

        #endregion
    }
}
