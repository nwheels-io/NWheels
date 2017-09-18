namespace NWheels.Microservices.Runtime
{
    public enum MicroserviceState
    {
        Down,
        Configuring,
        Configured,
        Loading,
        Standby,
        Activating,
        Active,
        Deactivating,
        Unloading
    }
}
