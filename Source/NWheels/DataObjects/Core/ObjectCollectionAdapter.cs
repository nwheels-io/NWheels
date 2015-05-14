using System;
using System.Collections.Generic;

namespace NWheels.DataObjects.Core
{
    public class ObjectCollectionAdapter<TConcrete, TAbstract> : ConcreteToAbstractCollectionAdapter<TConcrete, TAbstract>, IObjectCollection<TAbstract>
        where TConcrete : TAbstract
    {
        private readonly Func<TConcrete> _itemFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCollectionAdapter(ICollection<TConcrete> innerCollection, Func<TConcrete> itemFactory)
            : base(innerCollection)
        {
            _itemFactory = itemFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract Add()
        {
            var item = _itemFactory();
            base.InnerCollection.Add(item);
            return item;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract Insert(CollectionItemPosition position)
        {
            throw new NotImplementedException();
        }
    }
}
