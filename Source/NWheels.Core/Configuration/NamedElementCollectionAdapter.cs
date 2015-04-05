using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.Core.DataObjects;
using NWheels.Extensions;

namespace NWheels.Core.Configuration
{
    public class NamedElementCollectionAdapter<TConcrete, TAbstract> : CollectionAdapter<TConcrete, TAbstract>, INamedElementCollection<TAbstract>
        where TAbstract : class
        where TConcrete : TAbstract
    {
        public NamedElementCollectionAdapter(ICollection<TConcrete> innerCollection)
            : base(innerCollection)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetElementByName(string name, out TAbstract element)
        {
            element = (TAbstract)base.InnerCollection.Cast<INamedConfigurationElement>().FirstOrDefault(e => e.Name.EqualsIgnoreCase(name));
            return (element != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAbstract this[string name]
        {
            get
            {
                return (TAbstract)base.InnerCollection.Cast<INamedConfigurationElement>().First(e => e.Name.EqualsIgnoreCase(name));
            }
        }
    }
}
