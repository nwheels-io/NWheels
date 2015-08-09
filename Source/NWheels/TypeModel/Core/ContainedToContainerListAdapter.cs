using System.Collections.Generic;
using NWheels.TypeModel.Core;

namespace NWheels.DataObjects.Core
{
    public class ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract> :
        ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>, 
        IList<TContainerContract>,
        IReadOnlyList<TContainerContract>
        where TContained : class, IContainedIn<TContainer>
        where TContainer : class, IContain<TContained>
        where TContainedContract : class
        where TContainerContract : class
    {
        private readonly IList<TContainedContract> _innerList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContainedToContainerListAdapter(IList<TContainedContract> innerList)
            : base(innerList)
        {
            _innerList = innerList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int IndexOf(TContainerContract item)
        {
            return _innerList.IndexOf((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(int index, TContainerContract item)
        {
            _innerList.Insert(index, (TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContainerContract this[int index]
        {
            get
            {
                return (TContainerContract)(object)((IContainedIn<TContainer>)_innerList[index]).GetContainerObject();
            }
            set
            {
                _innerList[index] = (TContainedContract)(object)((IContain<TContained>)value).GetContainedObject();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IList<TContainedContract> InnerList
        {
            get
            {
                return _innerList;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract> CreateList(
            IList<TContainedContract> innerCollection)
        {
           return new ContainedToContainerListAdapter<TContained, TContainer, TContainedContract, TContainerContract>(innerCollection);
        }
    }
}