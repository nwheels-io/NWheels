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
    public class NewAlbumTx : TransactionScript<Empty.Input, NewAlbumTx.INewAlbumModel, List<NewAlbumTrackTx.INewTrackModel>>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public NewAlbumTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override INewAlbumModel InitializeInput(Empty.Input context)
        {
            using (_framework.NewUnitOfWork<IMusicDBContext>())
            {
                INewAlbumModel existingDraft;

                if (base.TryLoadInputDraft(out existingDraft))
                {
                    return WorkAroundLazyLoad(existingDraft);
                }

                var newDraft = _viewModelFactory.NewEntity<INewAlbumModel>();
                return WorkAroundLazyLoad(newDraft);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override List<NewAlbumTrackTx.INewTrackModel> Execute(INewAlbumModel input)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                var album = context.Albums.New();
                
                album.Name = input.AlbumName;
                album.Artist = input.Artist;
                album.ReleaseYear = input.ReleaseYear;

                context.Albums.Insert(album);
                
                base.DiscardInputDraft();
                context.CommitChanges();

                var trackList = Enumerable
                    .Range(1, 10)
                    .Select(num => {
                        var track = _viewModelFactory.NewEntity<NewAlbumTrackTx.INewTrackModel>();
                        track.TrackNumber = num;
                        track.Name = "Enter track name";
                        track.Description = "Enter track description";
                        track.Length = TimeSpan.FromMinutes(5);
                        track.TemporaryKey = _framework.NewGuid();
                        track.MoreInfoLinkText = "Find more info";
                        track.MoreInfoQuery = input.Artist.Name + " " + album.Name;
                        return track;
                    })
                    .ToList();

                return trackList;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private INewAlbumModel WorkAroundLazyLoad(INewAlbumModel model)
        {
            var loaded1 = model.Artist; 
            return model;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface INewAlbumModel
        {
            [PropertyContract.Required]
            string AlbumName { get; set; }

            [PropertyContract.Required]
            IArtistEntity Artist { get; set; }

            [PropertyContract.Validation.MinValue(0)]
            int ReleaseYear { get; set; }
        }
    }
}
