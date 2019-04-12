namespace NWheels.UI.Model
{
    public interface IBackendApiProxy<TApi>
    {
        TApi Api { get; }
    }
}