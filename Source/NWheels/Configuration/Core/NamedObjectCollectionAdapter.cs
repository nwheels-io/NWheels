using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Extensions;

namespace NWheels.Configuration.Core
{
    public class NamedObjectCollectionAdapter<TConcrete, TAbstract> : CollectionAdapter<TConcrete, TAbstract>, INamedObjectCollection<TAbstract>
        where TAbstract : class
        where TConcrete : TAbstract
    {
        private readonly Func<TConcrete> _itemFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NamedObjectCollectionAdapter(ICollection<TConcrete> innerCollection, Func<TConcrete> itemFactory)
            : base(innerCollection)
        {
            _itemFactory = itemFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetElementByName(string name, out TAbstract element)
        {
            element = (TAbstract)base.InnerCollection.Cast<INamedConfigurationElement>().FirstOrDefault(e => e.Name.EqualsIgnoreCase(name));
            return (element != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract Add(string name)
        {
            if ( base.InnerCollection.OfType<INamedConfigurationElement>().Any(e => e.Name.EqualsIgnoreCase(name)) )
            {
                throw new InvalidOperationException("Element with specified name alredy exists: " + name);
            }

            var item = _itemFactory();
            ((INamedConfigurationElement)item).Name = name;
            base.InnerCollection.Add(item);

            return item;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract Insert(string name, CollectionItemPosition position)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract this[string name]
        {
            get
            {
                var item = base.InnerCollection.Cast<INamedConfigurationElement>().FirstOrDefault(e => e.Name.EqualsIgnoreCase(name));

                if ( item != null )
                {
                    return (TAbstract)item;
                }
                else
                {
                    throw new KeyNotFoundException("Element with specified name does not exist: " + name);
                }
            }
        }
    }
}
