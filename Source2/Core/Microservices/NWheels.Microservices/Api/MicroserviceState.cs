namespace NWheels.Microservices.Api
{
    public enum MicroserviceState
    {
        /// <summary>
        /// Error state reached as a result of faulted operation. Instance in this state is no longer usable.
        /// </summary>
        Faulted,
        /// <summary>
        /// Original state of source project, after `dotnet build` or `dotnet publish`.
        /// </summary>
        Source,
        /// <summary>
        /// Configuration procedure is being performed
        /// </summary>
        Configuring,
        /// <summary>
        /// Microservice configuration is completed
        /// </summary>
        Configured,
        /// <summary>
        /// Code generation and compilation is being performed
        /// </summary>
        Compiling,
        /// <summary>
        /// Code generation and compilation are completed; 
        /// the service is ready to run in precompiled mode
        /// </summary>
        CompiledStopped,
        /// <summary>
        /// Load phase is being performed
        /// </summary>
        Loading,
        /// <summary>
        /// Microservice is in the standby mode (backup/follower replica in HA cluster)
        /// </summary>
        Standby,
        /// <summary>
        /// Activation phase is being performed
        /// </summary>
        Activating,
        /// <summary>
        /// Microservice is in the active mode (primary/leader replica in HA cluster)
        /// </summary>
        Active,
        /// <summary>
        /// Transitioning from Active to Standby
        /// </summary>
        Deactivating,
        /// <summary>
        /// Transitioning from Standby to CompiledStopped
        /// </summary>
        Unloading
    }
}
