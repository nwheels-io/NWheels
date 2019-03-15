namespace NWheels.UI.Model
{
    public static class DataSource<T>
    {
        public static IDataSourceFactory<T> GetFactories() => default;
    }

    public interface IDataSource<T>
    {
    }
    
    public interface IDataSourceFactory<T>
    {
        IDataSource<T> Transient { get; }
    }
}
