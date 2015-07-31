using System;
using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
    public class ConcreteToAbstractCollectionAdapter<TConcrete, TAbstract> : 
        ICollection<TAbstract>,
        IReadOnlyCollection<TAbstract>
        where TConcrete : TAbstract
    {
        private readonly ICollection<TConcrete> _innerCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConcreteToAbstractCollectionAdapter(ICollection<TConcrete> innerCollection)
        {
            _innerCollection = innerCollection;
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

        public ICollection<TConcrete> InnerCollection
        {
            get
            {
                return _innerCollection;
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public class ConcreteToAbstractListAdapter<TConcrete, TAbstract> : 
        ConcreteToAbstractCollectionAdapter<TConcrete, TAbstract>, 
        IList<TAbstract>,
        IReadOnlyList<TAbstract>
        where TConcrete : TAbstract
    {
        private readonly IList<TConcrete> _innerList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public ConcreteToAbstractListAdapter(IList<TConcrete> innerList)
            : base(innerList)
        {
            _innerList = innerList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int IndexOf(TAbstract item)
        {
            return _innerList.IndexOf((TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(int index, TAbstract item)
        {
            _innerList.Insert(index, (TConcrete)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract this[int index]
        {
            get
            {
                return _innerList[index];
            }
            set
            {
                _innerList[index] = (TConcrete)value;
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
    }
}
