using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityRelationalMapping<TEntity> where TEntity : class
    {
        IEntityRelationalMapping<TEntity> Table(string tableName);
        IEntityRelationalMapping<TEntity> Column(Expression<Func<TEntity, object>> property, string columnName = null, string dataType = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class EntityRelationalMappingConfiguration<TEntity> where TEntity : class
    {
        private readonly Action<IEntityRelationalMapping<TEntity>> _configurator;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityRelationalMappingConfiguration(Action<IEntityRelationalMapping<TEntity>> configurator)
        {
            _configurator = configurator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Action<IEntityRelationalMapping<TEntity>> Configurator
        {
            get { return _configurator; }
        }
    }
}
