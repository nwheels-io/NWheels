using System;
using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
    public class ConcreteToAbstractCollectionAdapter<TConcrete, TAbstract> : 
        ICollection<TAbstract>, 
        IList<TAbstract>, 
        IReadOnlyCollection<TAbstract>, 
        IReadOnlyList<TAbstract>
        where TConcrete : TAbstract
    {
        private readonly ICollection<TConcrete> _innerCollection;
        private readonly IList<TConcrete> _innerList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConcreteToAbstractCollectionAdapter(ICollection<TConcrete> innerCollection)
        {
            _innerCollection = innerCollection;
            _innerList = (innerCollection as IList<TConcrete>);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(TAbstract item)
        {
            _innerCollection.Add((TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _innerCollection.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Contains(TAbstract item)
        {
            return _innerCollection.Contains((TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyTo(TAbstract[] array, int arrayIndex)
        {
            var items = new TConcrete[_innerCollection.Count];
            _innerCollection.CopyTo(items, 0);

            for ( int i = 0 ; i < items.Length ; i++ )
            {
                array[i + arrayIndex] = items[i];
            }
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

        public bool Remove(TAbstract item)
        {
            return _innerCollection.Remove((TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TAbstract> GetEnumerator()
        {
            return (IEnumerator<TAbstract>)_innerCollection.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int IndexOf(TAbstract item)
        {
            ValidateInnerList();
            return _innerList.IndexOf((TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(int index, TAbstract item)
        {
            ValidateInnerList();
            _innerList.Insert(index, (TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RemoveAt(int index)
        {
            ValidateInnerList();
            _innerList.RemoveAt(index);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract this[int index]
        {
            get
            {
                ValidateInnerList();
                return _innerList[index];
            }
            set
            {
                ValidateInnerList();
                _innerList[index] = (TConcrete)value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ICollection<TConcrete> InnerCollection
        {
            get
            {
                return (_innerList ?? _innerCollection);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IList<TConcrete> InnerList
        {
            get
            {
                return _innerList;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void ValidateInnerList()
        {
            if ( _innerList == null )
            {
                throw new InvalidOperationException("The inner collection is not an IList<T>.");
            }
        }
    }
}
