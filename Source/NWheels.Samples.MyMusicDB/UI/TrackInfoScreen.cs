using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyMusicDB.UI
{
    public class TrackInfoScreen : ScreenBase<TrackInfoScreen, TrackInfoScreen.IInput, Empty.Data, Empty.State>
    {
        public TrackInfoScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenBase<TrackInfoScreen,IInput,Data,State>

        protected override void DescribePresenter(PresenterBuilder<TrackInfoScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.DashboardAdmin;

            TabSet.Tabs.Add(TrackInfoSearch);

            presenter.On(NavigatedHere)
                .Broadcast(TrackInfoSearch.NavigatedHere)
                    .WithPayload(vm => vm.Input)
                    .TunnelDown();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ContentRoot]
        public TabbedScreenPartSet TabSet { get; set; }
        public TrackInfoSearchScreenPart TrackInfoSearch { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IInput
        {
            string ArtistAlbumText { get; set; }
            string TrackText { get; set; }
        }
    }
}