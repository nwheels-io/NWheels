namespace NWheels.Hosting.Core
{
    internal enum NodeTrigger
    {
        Load,
        LoadSuccess,
        LoadFailure,
        Activate,
        ActivateSuccess,
        ActivateFailure,
        Deactivate,
        DeactivateDone,
        Unload,
        UnloadDone
    }
}
