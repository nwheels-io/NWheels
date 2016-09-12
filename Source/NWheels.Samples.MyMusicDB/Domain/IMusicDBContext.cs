using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Samples.MyMusicDB.Authorization;
using NWheels.Stacks.MongoDb;

namespace NWheels.Samples.MyMusicDB.Domain
{
    public interface IMusicDBContext : IApplicationDataRepository, IAutoIncrementIdDataRepository, IUserAccountDataRepository
    {
        IEntityRepository<IGenreEntity> Genres { get; }
        IEntityRepository<IArtistEntity> Artists { get; }
        IEntityRepository<IAlbumEntity> Albums { get; }
        IEntityRepository<ITrackEntity> Tracks { get; }

        IMusicDBUserAccountEntity NewUserAccount();
        IGenreRelation NewGenreRelation(IGenreEntity genre);

        IAdministratorAcl NewAdministratorAcl();
        IOperatorAcl NewOperatorAcl();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IMusicDBUserAccountEntity : IUserAccountEntity, IEntityPartUserAccountProfilePhoto
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IGenreEntity
    {
        [PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string Name { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IArtistEntity
    {
        [PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string Name { get; set; }

        [PropertyContract.Relation.Composition]
        ICollection<IGenreRelation> Genres { get; }

        [PropertyContract.Semantic.MultilineText]
        string Description { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IAlbumEntity
    {
        [PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Relation.CompositionParent]
        IArtistEntity Artist { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string Name { get; set; }

        [PropertyContract.Semantic.ImageUrl]
        string CoverImageUrl { get; set; }

        [PropertyContract.Required]
        int ReleaseYear { get; set; }

        [PropertyContract.Relation.Composition, PropertyContract.Storage.EmbeddedInParent(false)]
        ICollection<ITrackEntity> Tracks { get; }

        [PropertyContract.Semantic.MultilineText]
        string Description { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface ITrackEntity
    {
        [PropertyContract.AutoGenerate(typeof(AutoIncrementIntegerIdGenerator))]
        int Id { get; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string Name { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.CompositionParent]
        IAlbumEntity Album { get; set; }

        int TrackNumber { get; set; }

        TimeSpan Length { get; set; }

        [PropertyContract.Semantic.MultilineText]
        string Description { get; set; }

        [PropertyContract.Calculated]
        string AlbumName { get; }

        [PropertyContract.Calculated]
        int? AlbumYear { get; }

        [PropertyContract.Calculated]
        string ArtistName { get; }

        [PropertyContract.Calculated]
        string AlbumText { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TrackEntity : ITrackEntity
    {
        #region Implementation of ITrackEntity

        public abstract int Id { get; }
        public abstract string Name { get; set; }
        public abstract IAlbumEntity Album { get; set; }
        public abstract int TrackNumber { get; set; }
        public abstract TimeSpan Length { get; set; }
        public abstract string Description { get; set; }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AlbumName
        {
            get
            {
                return (Album != null ? Album.Name : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ArtistName
        {
            get
            {
                return (Album != null && Album.Artist != null ? Album.Artist.Name : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AlbumText
        {
            get
            {
                return (
                    Album != null && Album.Artist != null ? 
                    string.Format("{0}, {1} ({2})", Album.Artist.Name, Album.Name, Album.ReleaseYear) : 
                    null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int? AlbumYear
        {
            get
            {
                return (Album != null ? (int?)Album.ReleaseYear : null);
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityPartContract]
    public interface IGenreRelation
    {
        [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
        IGenreEntity Genre { get; set; }

        bool IsPrimary { get; set; }
        
        bool IsRecommended { get; set; }

        [PropertyContract.Calculated, PropertyContract.Semantic.DisplayName]
        string Text { get; }

        [PropertyContract.Calculated]
        string GenreName { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class GenreRelation : IGenreRelation
    {
        public abstract IGenreEntity Genre { get; set; }
        public abstract bool IsPrimary { get; set; }
        public abstract bool IsRecommended { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Text
        {
            get
            {
                if (Genre == null)
                {
                    return null;
                }

                var flags = new List<string>(2);

                if (IsPrimary)
                {
                    flags.Add("primary");
                }

                if (IsRecommended)
                {
                    flags.Add("recommended");
                }

                if (flags.Count > 0)
                {
                    return string.Format("{0} ({1})", Genre.Name, string.Join(", ", flags));
                }

                return Genre.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GenreName
        {
            get
            {
                return (Genre != null ? Genre.Name : null);
            }
        }
    }
}
