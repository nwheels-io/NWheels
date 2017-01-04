using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Globalization;
using NWheels.Globalization.Core;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Samples.MyMusicDB.UI
{
    public class NewAlbumScreenPart : ScreenPartBase<NewAlbumScreenPart, Empty.Input, Empty.Data, Empty.State>
    {
        public NewAlbumScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [ContentRoot]
        public Wizard Wizard { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<
            Empty.Input, 
            NewAlbumTx.INewAlbumModel, 
            NewAlbumTx, 
            List<NewAlbumTrackTx.INewTrackModel>> NewAlbumForm { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public DataGrid<NewAlbumTrackTx.INewTrackModel> NewTrackGrid { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<NewAlbumScreenPart, Empty.Data, Empty.State> presenter)
        {
            Wizard.Pages.Add(NewAlbumForm);
            Wizard.Pages.Add(NewTrackGrid);
            Wizard.GoNext(presenter.On(NewAlbumForm.OutputReady));

            NewTrackGrid.UseParentModelInputAsDataSource = true;
            NewTrackGrid.UseInlineEditor();
            
            presenter.On(NewTrackGrid.SavingInlineRowEdits)
                .InvokeTransactionScript<NewAlbumTrackTx>()
                .WaitForReply((tx, vm) => tx.Execute(vm.Input))
                .Then(
                    onSuccess: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.TrackSuccessfullySaved()),
                    onFailure: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.FailedToSaveTrack(), faultInfo: vm => vm.Input)
                );
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultCulture("en-US")]
        public interface IUserAlerts : IUserAlertRepository
        {
            [InfoAlert]
            UidlUserAlert TrackSuccessfullySaved();

            [ErrorAlert]
            UidlUserAlert FailedToSaveTrack();
        }
    }
}
