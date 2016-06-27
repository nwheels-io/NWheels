using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Globalization.Locales
{
    [EntityContract]
    public interface IApplicationLocaleEntity
    {
        [PropertyContract.EntityId, PropertyContract.Validation.Length(2, 5)]
        string IsoCode { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.Culture]
        string CultureCode { get; set; }

        [PropertyContract.Required, PropertyContract.Semantic.DisplayName]
        string EnglishName { get; set; }

        [PropertyContract.Relation.Composition, PropertyContract.Storage.EmbeddedInParent(false)]
        ICollection<IApplicationLocaleEntryEntity> Entries { get; set; }
    }
}
