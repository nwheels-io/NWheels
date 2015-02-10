using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using System.Linq.Expressions;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public abstract class EntityRelationalMappingBase<TEntityContract, TEntityImpl> : IEntityRelationalMapping<TEntityContract>
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        #region IEntityRelationalMapping<TEntityContract> Members

        public IEntityRelationalMapping<TEntityContract> Table(string tableName)
        {
            throw new NotImplementedException();
        }

        public IEntityRelationalMapping<TEntityContract> Column(Expression<Func<TEntityContract, object>> property, string columnName = null, string dataType = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
