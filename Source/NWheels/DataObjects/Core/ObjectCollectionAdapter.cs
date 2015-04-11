using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class ObjectCollectionAdapter<TConcrete, TAbstract> : CollectionAdapter<TConcrete, TAbstract>, IObjectCollection<TAbstract>
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
