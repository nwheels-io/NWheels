namespace NWheels.Api.Components
{
    public interface IDaemonComponent
    {
        void LoadingDaemons();
        void Load();
        void DaemonsLoaded();
        void ActivatingDaemons();
        void Activate();
        void DaemonsActivated();
        void MaybeDeactivatingDaemons();
        void MaybeDeactivate();
        void MaybeDaemonsDeactivated();
        void MaybeUnloadingDaemons();
        void MaybeUnload();
        void MaybeDaemonsUnloaded();
    }
}
