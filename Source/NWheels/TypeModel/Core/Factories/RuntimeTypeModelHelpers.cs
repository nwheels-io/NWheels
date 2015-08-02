using System.Collections.Generic;
using NWheels.DataObjects.Core;

namespace NWheels.TypeModel.Core.Factories
{
    public static class RuntimeTypeModelHelpers
    {
        public static object CreateCollectionAdapter<TConcrete, TAbstract>(object innerCollection, bool ordered) where TConcrete : TAbstract
        {
            if ( ordered )
            {
                return new ConcreteToAbstractListAdapter<TConcrete, TAbstract>((IList<TConcrete>)innerCollection);
            }
            else
            {
                return new ConcreteToAbstractCollectionAdapter<TConcrete, TAbstract>((ICollection<TConcrete>)innerCollection);
            }
        }
    }
}
