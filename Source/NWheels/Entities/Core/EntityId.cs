using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Core
{
    public class EntityId<TEntityContract, TValue> : IEntityId<TEntityContract>
        where TEntityContract : class
    {
        private readonly TValue _value;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityId(TValue value)
        {
            _value = value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEquatable<IEntityId>

        public bool Equals(IEntityId other)
        {
            var typedOther = (other as EntityId<TEntityContract, TValue>);

            if ( typedOther != null )
            {
                return (typedOther._value.Equals(this._value));
            }
            else
            {
                return false;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityId

        public Type ContractType
        {
            get
            {
                return typeof(TEntityContract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Value
        {
            get
            {
                return _value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T ValueAs<T>()
        {
            return (T)(object)_value;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override bool Equals(object obj)
        {
            var otherId = (obj as IEntityId);

            if ( otherId != null )
            {
                return this.Equals(otherId);
            }
            else
            {
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
