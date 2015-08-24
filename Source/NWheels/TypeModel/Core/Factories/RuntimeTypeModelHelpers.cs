using System.Collections.Generic;
using System.Linq;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DeepListNestedObject(object obj, HashSet<object> nestedObjects)
        {
            if ( obj != null )
            {
                if ( !nestedObjects.Add(obj) )
                {
                    return;
                }

                var hasNestedObjects = obj as IHaveNestedObjects;

                if ( hasNestedObjects != null )
                {
                    hasNestedObjects.DeepListNestedObjects(nestedObjects);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void DeepListNestedObjectCollection(System.Collections.IEnumerable collection, HashSet<object> nestedObjects)
        {
            if ( collection != null )
            {
                foreach ( var item in collection )
                {
                    if ( !nestedObjects.Add(item) )
                    {
                        continue;
                    }

                    var hasNestedObjects = item as IHaveNestedObjects;

                    if ( hasNestedObjects != null )
                    {
                        hasNestedObjects.DeepListNestedObjects(nestedObjects);
                    }
                }
            }
        }
    }
}
