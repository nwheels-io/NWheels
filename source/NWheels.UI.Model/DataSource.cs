namespace NWheels.UI.Model
{
    public static class DataSource<T>
    {
        public static IDataSourceFactory<T> GetFactories() => default;
    }

    public interface IDataSource<T>
    {
    }
    
    public interface IDataSourceFactory
    {
    }
    
    public interface IDataSourceFactory<T> : IDataSourceFactory
    {
        IDataSource<T> Transient { get; }
    }
}
