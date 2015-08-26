using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.TypeModel.Core
{
    public class InnerToOuterCollectionAdapter<T> :
        ICollection<T>,
        IReadOnlyCollection<T>
        where T : class
    {
        private readonly ICollection<T> _innerCollection;
        private readonly Func<T, T> _innerToOuter;
        private readonly Func<T, T> _outerToInner;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InnerToOuterCollectionAdapter(ICollection<T> innerCollection, Func<T, T> innerToOuter, Func<T, T> outerToInner)
        {
            _innerCollection = innerCollection;
            _innerToOuter = innerToOuter;
            _outerToInner = outerToInner;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(T item)
        {
            _innerCollection.Add(_outerToInner(item));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _innerCollection.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Contains(T item)
        {
            return _innerCollection.Contains(_outerToInner(item));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyTo(T[] array, int arrayIndex)
        {
            var items = new T[_innerCollection.Count];
            _innerCollection.CopyTo(items, 0);

            for ( int i = 0 ; i < items.Length ; i++ )
            {
                array[i + arrayIndex] = _innerToOuter(items[i]);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Remove(T item)
        {
            return _innerCollection.Remove(_outerToInner(item));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<T> GetEnumerator()
        {
            if ( _innerCollection == null )
            {
                return EnumerableUtility.GetEmptyEnumerator<T>();
            }

            return new DelegatingTransformingEnumerator<T, T>(
                _innerCollection.GetEnumerator(), 
                _innerToOuter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Count
        {
            get
            {
                return _innerCollection.Count;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsReadOnly
        {
            get
            {
                return _innerCollection.IsReadOnly;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ICollection<T> InnerCollection
        {
            get
            {
                return _innerCollection;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static InnerToOuterCollectionAdapter<T> CreateCollection(ICollection<T> innerCollection, Func<T, T> innerToOuter, Func<T, T> outerToInner)
        {
            return new InnerToOuterCollectionAdapter<T>(innerCollection, innerToOuter, outerToInner);
        }
    }
}
