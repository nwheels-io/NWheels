using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using NWheels.Domains.Security;
using NWheels.Domains.Security.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Documents;
using NWheels.Samples.MyMusicDB.Authorization;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.UI;
using NWheels.Utilities;

namespace NWheels.Samples.MyMusicDB.Deployment
{
    public class DemoDataPopulator : DomainContextPopulatorBase<IMusicDBContext>
    {
        private readonly IFrameworkUIConfig _uiConfig;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public DemoDataPopulator(IFrameworkUIConfig uiConfig)
        {
            _uiConfig = uiConfig;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DomainContextPopulatorBase<IHRContext>

        protected override void OnPopulateContext(IMusicDBContext context)
        {
            var rockGenre = AddGenre(context, "Rock");
            var jazzGenre = AddGenre(context, "Jazz");
            var popGenre = AddGenre(context, "Pop");
            var hipHopRapGenre = AddGenre(context, "Hip Hop / Rap");
            var bluesGenre = AddGenre(context, "Blues");
            var classicalGenre = AddGenre(context, "Classical Music");
            var operaGenre = AddGenre(context, "Opera");
            var countryGenre = AddGenre(context, "Country");
            var technoGenre = AddGenre(context, "Techno");
            var tranceGenre = AddGenre(context, "Trance");
            var alternativeGenre = AddGenre(context, "Alternative Music");
            var psychedelicGenre = AddGenre(context, "Psychedelic");
            var rnbSoulGenre = AddGenre(context, "R&B Soul");
            var newAgeGenre = AddGenre(context, "New Age");
            var latinGenre = AddGenre(context, "Latin");
            var childrensGenre = AddGenre(context, "Childrens Music");

            var pinkFloyd = AddArtist(context, "Pink Floyd", rockGenre, psychedelicGenre);
            var cranberries = AddArtist(context, "Cranberries", rockGenre, alternativeGenre);
            var nirvana = AddArtist(context, "Nirvana", rockGenre);

            AddAlbum(context, nirvana, "Nevermind", 1991,
                NewTrack(context, "Smells Like Teen Spirit", "05:01"),
                NewTrack(context, "In Bloom", "04:14"),
                NewTrack(context, "Come as You Are", "03:39"),
                NewTrack(context, "Breed", "03:03"),
                NewTrack(context, "Lithium", "04:17"),
                NewTrack(context, "Polly", "02:57"),
                NewTrack(context, "Territorial Pissings", "02:22"),
                NewTrack(context, "Drain You", "03:43"),
                NewTrack(context, "Lounge Act", "02:36"),
                NewTrack(context, "Stay Away", "03:32"),
                NewTrack(context, "On a Plain", "03:16"),
                NewTrack(context, "Something in the Way", "03:46"));

            AddAlbum(context, pinkFloyd, "The Division Bell", 1994,
                NewTrack(context, "Cluster One (Instrumental)", "5:58"),
                NewTrack(context, "What Do You Want from Me", "4:21"),
                NewTrack(context, "Poles Apart", "7:04"),
                NewTrack(context, "Marooned (Instrumental)", "5:29"),
                NewTrack(context, "A Great Day for Freedom", "4:17"),
                NewTrack(context, "Wearing the Inside Out (Lead vocals: Wright, Gilmour)", "0:49"),
                NewTrack(context, "Take It Back", "6:12"),
                NewTrack(context, "Coming Back to Life", "6:19"),
                NewTrack(context, "Keep Talking", "6:11"),
                NewTrack(context, "Lost for Words", "5:14"),
                NewTrack(context, "High Hopes", "8:31"));

            AddAlbum(context, cranberries, "Bury The Hatchet", 1999,
                NewTrack(context, "Animal Instinct", "3:31"),
                NewTrack(context, "Loud And Clear", "2:45"),
                NewTrack(context, "Promises", "5:27"),
                NewTrack(context, "You And Me", "3:35"),
                NewTrack(context, "Just My Imagination", "3:41"),
                NewTrack(context, "Shattered", "3:41"),
                NewTrack(context, "Desperate Andy", "3:44"),
                NewTrack(context, "Saving Grace", "3:08"),
                NewTrack(context, "Copycat", "2:53"),
                NewTrack(context, "What's On My Mind", "3:12"),
                NewTrack(context, "Delilah", "3:32"),
                NewTrack(context, "Fee Fi Fo", "4:47"),
                NewTrack(context, "Dying In The Sun", "3:32"),
                NewTrack(context, "Sorry Son", "3:25"));
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IGenreEntity AddGenre(IMusicDBContext context, string name)
        {
            var genre = context.Genres.New();
            genre.Name = name;
            context.Genres.Insert(genre);
            return genre;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IArtistEntity AddArtist(IMusicDBContext context, string name, params IGenreEntity[] genres)
        {
            var artist = context.Artists.New();
            artist.Name = name;
            artist.Description = "More info will be available soon.";

            for (int i = 0 ; i < genres.Length ; i++)
            {
                var artistGenre = context.NewGenreRelation(genres[i]);
                artistGenre.IsPrimary = (i == 0);
                artistGenre.IsRecommended = (i < 2);
                artist.Genres.Add(artistGenre);
            }
            
            context.Artists.Insert(artist);
            return artist;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IAlbumEntity AddAlbum(IMusicDBContext context, IArtistEntity artist, string name, int year, params ITrackEntity[] tracks)
        {
            var album = context.Albums.New();
            album.Artist = artist;
            album.Name = name;
            album.ReleaseYear = year;
            album.Description = "More info will be available soon.";

            for (int i = 0; i < tracks.Length; i++)
            {
                tracks[i].Album = album;
                tracks[i].TrackNumber = i + 1;
                album.Tracks.Add(tracks[i]);
                context.Tracks.Insert(tracks[i]);
            }

            context.Albums.Insert(album);
            return album;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static ITrackEntity NewTrack(IMusicDBContext context, string name, string length)
        {
            TimeSpan parsedLength;

            if (!TimeSpan.TryParseExact(length, "mm\\:ss", CultureInfo.InvariantCulture, out parsedLength))
            {
                parsedLength = TimeSpan.ParseExact(length, "m\\:ss", CultureInfo.InvariantCulture);
            }

            var track = context.Tracks.New();
            track.Name = name;
            track.Length = parsedLength;
            track.Description = "More info will be available soon.";
            return track;
        }
    }
}