using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Samples.MyMusicDB.Domain
{
    [TransactionScript(SupportsInitializeInput = true, SupportsInputDraft = true)]
    public class NewAlbumTrackTx : TransactionScript<NewAlbumTx.INewAlbumModel, NewAlbumTrackTx.INewTrackModel, Empty.Output>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NewAlbumTrackTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override INewTrackModel InitializeInput(NewAlbumTx.INewAlbumModel context)
        {
            var newTrack = _viewModelFactory.NewEntity<INewTrackModel>();
            newTrack.Id = _framework.NewGuid();
            newTrack.TrackNumber = 0;
            newTrack.Name = "Enter track name";
            newTrack.Description = "Enter track description";
            return newTrack;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Empty.Output Execute(INewTrackModel input)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                var track = context.Tracks.New();

                //track.Album = album;
                track.Name = input.Name;
                track.TrackNumber = input.TrackNumber;
                track.Length = input.Length;
                track.Description = input.Description;

                var temporaryKey = input.TemporaryKey; // example of hidden column
                
                context.Tracks.Insert(track);
            }

            base.DiscardInputDraft();
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INewTrackModel Approve(INewTrackModel input)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                var track = context.Tracks.New();

                //track.Album = album;
                track.Name = input.Name;
                track.TrackNumber = input.TrackNumber;
                track.Length = input.Length;
                track.Description = input.Description;

                var temporaryKey = input.TemporaryKey; // example of hidden column

                context.Tracks.Insert(track);

            }

            base.DiscardInputDraft();

            input.Status = NewAlbumTrackStatus.Approved;

            var historyNote = _viewModelFactory.NewEntity<NewAlbumTrackTx.INewTrackModelHistoryNote>();
            historyNote.Who = Session.Current.GetUserAccountAs<IUserAccountEntity>().LoginName;
            historyNote.When = _framework.UtcNow;
            historyNote.What = "The track was approved.";
            input.History.Add(historyNote);

            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public INewTrackModel Reject(INewTrackModel input)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                var track = context.Tracks.New();

                //track.Album = album;
                track.Name = input.Name;
                track.TrackNumber = input.TrackNumber;
                track.Length = input.Length;
                track.Description = input.Description;

                var temporaryKey = input.TemporaryKey; // example of hidden column

                context.Tracks.Insert(track);
            }

            base.DiscardInputDraft();

            input.Status = NewAlbumTrackStatus.Rejected;
            
            var historyNote = _viewModelFactory.NewEntity<NewAlbumTrackTx.INewTrackModelHistoryNote>();
            historyNote.Who = Session.Current.GetUserAccountAs<IUserAccountEntity>().LoginName;
            historyNote.When = _framework.UtcNow;
            historyNote.What = "The track was rejected.";
            input.History.Add(historyNote);

            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum NewAlbumTrackStatus
        {
            Pending,
            Approved,
            Rejected
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface INewTrackModel
        {
            [PropertyContract.EntityId]
            Guid Id { get; set; }

            [PropertyContract.Presentation.Hidden]
            Guid TemporaryKey { get; set; }

            [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
            string Name { get; set; }

            [PropertyContract.Semantic.OrderBy, PropertyContract.Validation.MinValue(1)]
            int TrackNumber { get; set; }

            TimeSpan Length { get; set; }

            string Description { get; set; }

            [PropertyContract.ReadOnly]
            string MoreInfoLinkText { get; set; }

            [PropertyContract.Presentation.Hidden]
            string MoreInfoQuery { get; set; }

            NewAlbumTrackStatus Status { get; set; }

            [PropertyContract.Relation.Composition]
            ICollection<INewTrackModelHistoryNote> History { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface INewTrackModelHistoryNote
        {
            DateTime When { get; set; }
            string Who { get; set; }
            string What { get; set; }
        }
    }
}
