namespace NWheels.Microservices
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
