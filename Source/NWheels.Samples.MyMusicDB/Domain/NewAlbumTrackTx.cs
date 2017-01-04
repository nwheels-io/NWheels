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
                context.Tracks.Insert(track);
            }

            base.DiscardInputDraft();
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityPartContract]
        public interface INewTrackModel
        {
            [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
            string Name { get; set; }

            [PropertyContract.Semantic.OrderBy, PropertyContract.Validation.MinValue(1)]
            int TrackNumber { get; set; }

            TimeSpan Length { get; set; }

            [PropertyContract.Semantic.MultilineText]
            string Description { get; set; }
        }
    }
}
