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
    public class ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract> :
        ICollection<TContainerContract>,
        IReadOnlyCollection<TContainerContract>,
        IChangeTrackingCollection<TContainerContract>
        where TContained : class, IContainedIn<TContainer>
        where TContainer : class, IContain<TContained>
        where TContainedContract : class
        where TContainerContract : class
    {
        private readonly ICollection<TContainedContract> _innerCollection;
        private readonly Func<TContained, TContainer> _containerFactory;
        private List<TContainerContract> _added = null;
        private List<TContainerContract> _removed = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContainedToContainerCollectionAdapter(ICollection<TContainedContract> innerCollection)
            : this(innerCollection, containerFactory: null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContainedToContainerCollectionAdapter(ICollection<TContainedContract> innerCollection, Func<TContained, TContainer> containerFactory)
        {
            _innerCollection = innerCollection;
            _containerFactory = containerFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Add(TContainerContract item)
        {
            _innerCollection.Add((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());

            if ( _added == null )
            {
                _added = new List<TContainerContract>(capacity: 4);
            }

            _added.Add(item);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            if ( _removed == null )
            {
                _removed = new List<TContainerContract>();
            }

            _removed.AddRange(_innerCollection
                .Cast<TContained>()
                .Select(c => c.GetContainerObject() ?? GetContainerFromContained(c))
                .Cast<TContainerContract>()
                .Where(c => _added == null || !_added.Contains(c)));
            _added = null;
            
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
                array[i + arrayIndex] = (TContainerContract)(object)GetContainerFromContained((TContained)(object)items[i]);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Remove(TContainerContract item)
        {
            var itemWasRemoved = _innerCollection.Remove((TContainedContract)(object)((IContain<TContained>)item).GetContainedObject());

            if ( itemWasRemoved )
            {
                var itemWasAdded = (_added != null && _added.Remove(item));

                if ( !itemWasAdded )
                { 
                    if ( _removed == null )
                    {
                        _removed = new List<TContainerContract>(capacity: 4);
                    }

                    _removed.Add(item);
                }
            }

            return itemWasRemoved;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TContainerContract> GetEnumerator()
        {
            if ( _innerCollection == null )
            {
                return EnumerableUtility.GetEmptyEnumerator<TContainerContract>();
            }

            return new DelegatingTransformingEnumerator<TContainedContract, TContainerContract>(
                _innerCollection.GetEnumerator(), 
                ContainedContractToContainerContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GetChanges(out TContainerContract[] added, out TContainerContract[] changed, out TContainerContract[] removed)
        {
            var modifiedList = FindModifiedItems();

            changed = (modifiedList != null ? modifiedList.ToArray() : null);
            added = (_added != null ? _added.ToArray() : null);
            removed = (_removed != null ? _removed.ToArray() : null);
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

        public bool IsChanged
        {
            get
            {
                return (
                    _added != null || 
                    _removed != null || 
                    (_innerCollection != null && _innerCollection.Cast<TContained>().Any(IsItemModified)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TContainerContract ContainedContractToContainerContract(TContainedContract contained)
        {
            return (TContainerContract)(object)GetContainerFromContained((TContained)(object)contained);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TContainer GetContainerFromContained(TContained contained)
        {
            var existingContainer = contained.GetContainerObject();

            if ( existingContainer != null )
            {
                return existingContainer;
            }

            if ( _containerFactory != null )
            {
                var newContainer = _containerFactory(contained);
                return newContainer;
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<TContainerContract> FindModifiedItems()
        {
            List<TContainerContract> changedList = null;

            foreach ( var containedItem in _innerCollection.Cast<TContained>() )
            {
                if ( IsItemModified(containedItem) )
                {
                    if ( changedList == null )
                    {
                        changedList = new List<TContainerContract>(capacity: 4);
                    }

                    changedList.Add((TContainerContract)(object)containedItem.GetContainerObject());
                }
            }

            return changedList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsItemModified(TContained containedItem)
        {
            var containerItem = containedItem.GetContainerObject();
            var containerObject = containerItem as IObject;

            return (containerObject != null && containerObject.IsModified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract> CreateCollection(
            ICollection<TContainedContract> innerCollection)
        {
            return new ContainedToContainerCollectionAdapter<TContained, TContainer, TContainedContract, TContainerContract>(innerCollection);
        }
    }
}
