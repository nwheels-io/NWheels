using System;
using System.Linq.Expressions;

namespace NWheels.Entities
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
