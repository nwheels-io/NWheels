using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core
{
    public class AbstractToConcreteCollectionAdapter<TAbstract, TConcrete> :
        ICollection<TConcrete>, 
        IList<TConcrete>, 
        IReadOnlyCollection<TConcrete>, 
        IReadOnlyList<TConcrete>
        where TConcrete : TAbstract
    {
        private readonly ICollection<TAbstract> _innerCollection;
        private readonly IList<TAbstract> _innerList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AbstractToConcreteCollectionAdapter(ICollection<TAbstract> innerCollection)
        {
            _innerCollection = innerCollection;
            _innerList = (innerCollection as IList<TAbstract>);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(TConcrete item)
        {
            _innerCollection.Add((TAbstract)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _innerCollection.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Contains(TConcrete item)
        {
            return _innerCollection.Contains((TAbstract)item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyTo(TConcrete[] array, int arrayIndex)
        {
            var items = new TAbstract[_innerCollection.Count];
            _innerCollection.CopyTo(items, 0);

            for ( int i = 0 ; i < items.Length ; i++ )
            {
                array[i + arrayIndex] = (TConcrete)items[i];
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

        public bool Remove(TConcrete item)
        {
            return _innerCollection.Remove(item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TConcrete> GetEnumerator()
        {
            return (IEnumerator<TConcrete>)_innerCollection.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerCollection.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int IndexOf(TConcrete item)
        {
            ValidateInnerList();
            return _innerList.IndexOf(item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(int index, TConcrete item)
        {
            ValidateInnerList();
            _innerList.Insert(index, item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RemoveAt(int index)
        {
            ValidateInnerList();
            _innerList.RemoveAt(index);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcrete this[int index]
        {
            get
            {
                ValidateInnerList();
                return (TConcrete)_innerList[index];
            }
            set
            {
                ValidateInnerList();
                _innerList[index] = (TAbstract)value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ICollection<TAbstract> InnerCollection
        {
            get
            {
                return _innerCollection;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected IList<TAbstract> InnerList
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
