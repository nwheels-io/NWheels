#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public class EntityId<TEntity, TId> : IEntityId<TEntity>
        where TEntity : class
    {
        private TId _id1;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityId

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Equals(IEntityId other)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T GetValue<T>()
        {
            throw new NotImplementedException();
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
    }
}

#endif