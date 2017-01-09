using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyMusicDB.UI
{
    public class TrackInfoSearchScreenPart : ScreenPartBase<TrackInfoSearchScreenPart, TrackInfoScreen.IInput, Empty.Data, Empty.State>
    {
        public TrackInfoSearchScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<TrackInfoSearchScreenPart, TrackInfoScreen.IInput, Empty.Data, Empty.State>

        protected override void DescribePresenter(PresenterBuilder<TrackInfoSearchScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Search;

            Search.ApiKey = "AIzaSyCT2mXucoKvhwQWVhKj7jCnw0YNWTRtGMs";
            Search.EngineId = "016629047946340979967:zkxh5ynbyms";

            presenter.On(NavigatedHere)
                .Broadcast(Search.QuerySetter)
                    .WithPayload(vm => vm.Input.ArtistAlbumText)
                    .TunnelDown();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InternetSearch Search { get; set; }
    }
}
