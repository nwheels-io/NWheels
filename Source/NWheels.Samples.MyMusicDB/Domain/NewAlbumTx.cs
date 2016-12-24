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

namespace NWheels.Samples.MyMusicDB.Domain
{
    [TransactionScript(SupportsInitializeInput = true, SupportsInputDraft = true)]
    public class NewAlbumTx : TransactionScript<Empty.Input, NewAlbumTx.INewAlbumModel, Empty.Output>
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NewAlbumTx(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override INewAlbumModel InitializeInput(Empty.Input context)
        {
            using (_framework.NewUnitOfWork<IMusicDBContext>())
            {
                NewAlbumTx.INewAlbumModel existingDraft;

                if (TryLoadInputDraft(out existingDraft))
                {
                    var loaded1 = existingDraft.Artist; //work around lazy load
                    return existingDraft;
                }

                var newDraft = _framework.NewDomainObject<INewAlbumModel>();
                var loaded2 = newDraft.Artist; //work around lazy load
                return newDraft;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Empty.Output Execute(INewAlbumModel input)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                var album = context.Albums.New();
                album.Name = input.AlbumName;
                album.Artist = input.Artist;
                album.ReleaseYear = input.ReleaseYear;

                foreach (var inputTrack in input.Tracks)
                {
                    var track = context.Tracks.New();
                    track.Album = album;
                    track.Name = inputTrack.Name;
                    track.TrackNumber = inputTrack.TrackNumber;
                    track.Length = inputTrack.Length;
                    track.Description = inputTrack.Description;
                    context.Tracks.Insert(track);
                }

                context.Albums.Insert(album);
                context.CommitChanges();
            }

            base.DiscardInputDraft();
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityPartContract]
        public interface INewAlbumModel
        {
            [PropertyContract.Required]
            string AlbumName { get; set; }

            [PropertyContract.Required]
            IArtistEntity Artist { get; set; }

            [PropertyContract.Validation.MinValue(0)]
            int ReleaseYear { get; set; }

            [PropertyContract.Relation.Composition, PropertyContract.Storage.EmbeddedInParent(true)]
            ICollection<INewTrackModel> Tracks { get; }
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
