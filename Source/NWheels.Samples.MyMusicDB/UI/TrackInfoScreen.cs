using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.UI;
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
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IInput
        {
            string ArtistAlbumText { get; set; }
            string TrackText { get; set; }
        }
    }
}