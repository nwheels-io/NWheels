using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public class EntityId<TEntityContract, TId1> : IEntityId<TEntityContract>
        where TEntityContract : class
    {
        private readonly TId1 _id1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityId(TId1 id1)
        {
            _id1 = id1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEquatable<IEntityId>

        public bool Equals(IEntityId other)
        {
            var typedOther = (other as EntityId<TEntityContract, TId1>);

            if ( typedOther != null )
            {
                return (typedOther._id1.Equals(this._id1));
            }
            else
            {
                return false;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityId

        public Type GetContract()
        {
            return typeof(TEntityContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetValue()
        {
            return _id1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetValue<T>()
        {
            return (T)(object)_id1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Tuple<T1, T2> GetValue<T1, T2>()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Tuple<T1, T2, T3> GetValue<T1, T2, T3>()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Tuple<T1, T2, T3, T4> GetValue<T1, T2, T3, T4>()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
