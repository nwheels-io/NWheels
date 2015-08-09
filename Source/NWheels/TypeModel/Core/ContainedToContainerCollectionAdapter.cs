using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    public class ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract> :
        ICollection<TContainerContract>,
        IReadOnlyCollection<TContainerContract>
        where TContained : class, IContainedIn<TContainer>
        where TContainer : class, IContain<TContained>
        where TContainedContract : class
        where TContainerContract : class
    {
        private readonly ICollection<TContainedContract> _innerCollection;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContainedToContainerCollectionAdapter(ICollection<TContainedContract> innerCollection)
        {
            _innerCollection = innerCollection;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(TContainerContract item)
        {
            _innerCollection.Add((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _innerCollection.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Contains(TContainerContract item)
        {
            return _innerCollection.Contains((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyTo(TContainerContract[] array, int arrayIndex)
        {
            var items = new TContainedContract[_innerCollection.Count];
            _innerCollection.CopyTo(items, 0);

            for ( int i = 0 ; i < items.Length ; i++ )
            {
                array[i + arrayIndex] = (TContainerContract)(object)((IContainedIn<TContainer>)items[i]).GetContainerObject();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Remove(TContainerContract item)
        {
            return _innerCollection.Remove((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TContainerContract> GetEnumerator()
        {
            return (IEnumerator<TContainerContract>)_innerCollection.GetEnumerator();
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

        public ICollection<TContainedContract> InnerCollection
        {
            get
            {
                return _innerCollection;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract> CreateCollection(
            ICollection<TContainedContract> innerCollection)
        {
            return new ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(innerCollection);
        }
    }
}
