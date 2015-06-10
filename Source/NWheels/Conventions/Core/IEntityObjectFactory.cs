namespace NWheels.Conventions.Core
{
    public interface IEntityObjectFactory
    {
        TEntityContract NewEntity<TEntityContract>() where TEntityContract : class;
    }
}