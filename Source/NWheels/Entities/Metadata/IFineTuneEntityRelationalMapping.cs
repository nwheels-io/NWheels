using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IRelationalMappingFineTune<TEntity> where TEntity : class
    {
        IRelationalMappingFineTune<TEntity> Table(string tableName);
        IRelationalMappingFineTune<TEntity> Column(Expression<Func<TEntity, object>> property, string columnName = null, string dataType = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RelationalMappingFineTuner<TEntity> where TEntity : class
    {
        private readonly Action<IRelationalMappingFineTune<TEntity>> _fineTuneAction;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RelationalMappingFineTuner(Action<IRelationalMappingFineTune<TEntity>> fineTuneAction)
        {
            _fineTuneAction = fineTuneAction;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(IRelationalMappingFineTune<TEntity> fineTune)
        {
            _fineTuneAction(fineTune);
        }
    }
}
