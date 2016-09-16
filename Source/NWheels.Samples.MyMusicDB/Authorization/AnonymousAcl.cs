using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Authorization;
using NWheels.Samples.MyMusicDB.Domain;

namespace NWheels.Samples.MyMusicDB.Authorization
{
    public class AnonymousAcl : AnonymousEntityAccessRule
    {
        #region Overrides of AnonymousEntityAccessRule

        public override void BuildAccessControl(IEntityAccessControlBuilder access)
        {
            access.ToEntities<IGenreEntity, IArtistEntity, IAlbumEntity, ITrackEntity>().IsReadOnly();
        }

        #endregion
    }
}