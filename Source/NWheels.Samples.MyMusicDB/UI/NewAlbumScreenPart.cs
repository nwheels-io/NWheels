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
    /*
     * How to add screen part with two-step wizard. 
     * The 1st step is a transaction form (with a TX on server), whose output is a list of rows. 
     * The 2nd step is editable grid for rows created by step 1. The rows are saved to server through another TX.
     * 
     * Developer walkthrough:
     * 1. Create interactive TX for user's 1st step (NewAlbumTx); in the TX, define view model for the form (INewAlbumModel). 
     * 2. Create interactive TX for user's 2nd step (NewAlbumTrackTx); in the TX, define view model for grid row (INewTrackModel).
     * 3. Register both TX in ModuleLoader.
     * 4. Create screen part class (NewAlbumScreenPart), which includes Wizard, form for the 1st step (NewAlbumForm), and grid for the 2nd step (NewTrackGrid).
     * 
     */
    public class NewAlbumScreenPart : ScreenPartBase<NewAlbumScreenPart, Empty.Input, Empty.Data, Empty.State>
    {
        public NewAlbumScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        // this property is injected automatically during composition of UIDL document.
        // because it is of type derived from UidlApplication
        // it refers to the application UIDL object
        // this property allows us referring to other screens in the app - for instance, the Track Info screen
        public MusicDBApp TheApp { get; set; }
        
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

        public TabSet TrackDetailTabs { get; set; }
        public JsonText TrackDetailJson { get; set; }
        public StaticTable<NewAlbumTrackTx.INewTrackModelHistoryNote> TrackDetailHistory { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand<NewAlbumTrackTx.INewTrackModel> ApproveTrack { get; set; }
        public UidlCommand<NewAlbumTrackTx.INewTrackModel> RejectTrack { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<NewAlbumScreenPart, Empty.Data, Empty.State> presenter)
        {
            NewAlbumForm.UseFlatStyle();
            NewAlbumForm.DoNotHideInput = true;

            NewTrackGrid.Mode = DataGridMode.Standalone;
            NewTrackGrid.PassiveQueryMode = true;
            NewTrackGrid.UseInlineEditor();
            NewTrackGrid.DisableFiltering = true;
            NewTrackGrid.DisableSorting = true;
            NewTrackGrid.UseDetailPane(TrackDetailTabs);
            NewTrackGrid.InlineEditRowActions = DataGridRowActions.Revert;
            NewTrackGrid.ShowActionsOnSelectedRowOnly = true;
            NewTrackGrid.UseRowCommands(ApproveTrack, RejectTrack);
            NewTrackGrid.BindRowStyleTo(x => x.Status, valueMap: new Dictionary<object, string>() {
                 { NewAlbumTrackTx.NewAlbumTrackStatus.Pending, "default" },
                 { NewAlbumTrackTx.NewAlbumTrackStatus.Approved, "success" },
                 { NewAlbumTrackTx.NewAlbumTrackStatus.Rejected, "danger" }
            });
            NewTrackGrid
                .Column(x => x.TrackNumber, size: FieldSize.Small)
                .Column(x => x.Name, size: FieldSize.Large)
                .Column(x => x.Status, size: FieldSize.Small)
                .Column(x => x.Length, size: FieldSize.Small)
                .Column(x => x.Description, size: FieldSize.Large)
                .Column(x => x.MoreInfoLinkText, setup: c => c.Clickable = true)
                .Column(x => x.History, columnType: GridColumnType.Hidden)
                .Column(x => x.TemporaryKey, columnType: GridColumnType.Hidden);
            NewTrackGrid.InlineEditor.Field(x => x.Description, type: FormFieldType.Edit, modifiers: FormFieldModifiers.Memo);

            TrackDetailTabs.TemplateName = "TabSetInlineStyle"; // or "TabSetInlineStyleBottom" to display tabs in the bottom
            TrackDetailTabs.Tabs.Add(TrackDetailHistory);
            TrackDetailTabs.Tabs.Add(TrackDetailJson);

            TrackDetailJson.Text = "JSON";
            TrackDetailJson.ExpandedByDefault = false;

            TrackDetailHistory.Text = "History";
            TrackDetailHistory.TemplateName = "StaticTableInlineStyle";
            TrackDetailHistory.DescribingPresenter += (p) => p
                .On(TrackDetailHistory.Loaded)
                .QueryParentModelAs<NewAlbumTrackTx.INewTrackModel>()
                .Then(b => b.Broadcast(TrackDetailHistory.DataReceived).WithPayload(vm => vm.Input.History).TunnelDown());
            TrackDetailHistory
                .Column(x => x.When)
                .Column(x => x.Who)
                .Column(x => x.What);
            
            ApproveTrack.Icon = "check";
            RejectTrack.Icon = "times";

            Wizard.Pages.Add(NewAlbumForm);
            Wizard.Pages.Add(NewTrackGrid);

            Wizard.GoNext(presenter.On(NewAlbumForm.OutputReady));

            presenter.On(NewAlbumForm.OutputReady)
                .Broadcast(NewTrackGrid.RequestPrepared)
                .WithPayload(vm => vm.Input)
                .TunnelDown();

            //presenter.On(NewTrackGrid.SavingInlineRowEdits)
            //    .InvokeTransactionScript<NewAlbumTrackTx>()
            //    .WaitForReply((tx, vm) => tx.Execute(vm.Input))
            //    .Then(
            //        onSuccess: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.TrackSuccessfullySaved()),
            //        onFailure: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.FailedToSaveTrack(), faultInfo: vm => vm.Input)
            //    );

            presenter.On(ApproveTrack)
                .InvokeTransactionScript<NewAlbumTrackTx>()
                .WaitForReply((tx, vm) => tx.Approve(vm.Input))
                .Then(
                    onSuccess: b1 => b1
                        .Broadcast(NewTrackGrid.DataRowReceived).WithPayload(vm => vm.Input).TunnelDown()
                        .Then(b2 => b2
                            .UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.TrackApproved())),
                    onFailure: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.FailedToSaveTrack(), faultInfo: vm => vm.Input)
                );

            presenter.On(RejectTrack)
                .InvokeTransactionScript<NewAlbumTrackTx>()
                .WaitForReply((tx, vm) => tx.Reject(vm.Input))
                .Then(
                    onSuccess: b1 => b1
                        .Broadcast(NewTrackGrid.DataRowReceived).WithPayload(vm => vm.Input).TunnelDown()
                        .Then(b2 => b2
                            .UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.TrackApproved())),
                    onFailure: b => b.UserAlertFrom<IUserAlerts>().ShowPopup((ua, vm) => ua.FailedToSaveTrack(), faultInfo: vm => vm.Input)
                );

            presenter.On(NewTrackGrid.CellClicked)
                .ProjectInputAs<TrackInfoScreen.IInput>(
                    alt => alt.Copy(vm => vm.Input.Source.Data.MoreInfoQuery).To(vm => vm.Input.Target.ArtistAlbumText),
                    alt => alt.Copy(vm => vm.Input.Source.Data.Name).To(vm => vm.Input.Target.TrackText))
                .Then(b => b.Navigate()
                    .ToScreen(TheApp.TrackInfo, NavigationType.Popup)
                    .WithInput(vm => vm.Input.Target));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [DefaultCulture("en-US")]
        public interface IUserAlerts : IUserAlertRepository
        {
            [InfoAlert]
            UidlUserAlert TrackSuccessfullySaved();

            [SuccessAlert]
            UidlUserAlert TrackApproved();

            [SuccessAlert]
            UidlUserAlert TrackRejected();

            [ErrorAlert]
            UidlUserAlert FailedToSaveTrack();
        }
    }   
}
