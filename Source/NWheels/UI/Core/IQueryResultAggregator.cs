namespace NWheels.UI.Core
{
    public interface IQueryResultAggregator
    {
        void Aggregate(object record);
        object GetAggregatedValue(string propertyAliasName);
    }
}
