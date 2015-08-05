using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public class EntityStateInfo<TEntity> 
        where TEntity : class
    {
        public EntityState StateOf<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            return EntityState.RetrievedPristine;
        }

        public bool IsModified<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            return false;
        }

        public TProperty RetrievedValueOf<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            return default(TProperty);
        }

        public TProperty StoredValueOf<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            return default(TProperty);
        }
    }
}
