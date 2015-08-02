using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ConcreteToAbstractListAdapter<TConcrete, TAbstract> CreateList(IList<TConcrete> innerCollection)
        {
            return new ConcreteToAbstractListAdapter<TConcrete, TAbstract>(innerCollection);
        }
    }
}