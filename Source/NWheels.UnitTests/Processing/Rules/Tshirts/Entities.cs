using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.UnitTests.Processing.Rules.Tshirts
{
    [EntityContract]
    public interface ITshirtEntity
    {
        [PropertyContract.Required, PropertyContract.Relation.ManyToOne]
        IColorEntity Color { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.ManyToOne]
        ISizeEntity Size { get; set; }

        [PropertyContract.Required, PropertyContract.Relation.ManyToOne]
        IModelEntity Model { get; set; }
    }
    [EntityContract]
    public interface IColorEntity : IEntityPartUniqueDisplayName
    {
    }
    [EntityContract]
    public interface ISizeEntity : IEntityPartUniqueDisplayName
    {
    }
    [EntityContract]
    public interface IModelEntity : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Required, PropertyContract.Unique, PropertyContract.Validation.Length(5, 50)]
        string CatalogNo { get; set; }

        [PropertyContract.Relation.ManyToOne]
        IMaterialEntity Material { get; set; }

    }
    [EntityContract]
    public interface IMaterialEntity : IEntityPartUniqueDisplayName
    {
        [PropertyContract.Relation.Composition]
        ICollection<IClothComponentEntity> ClothPerMaterial { get; }
    }

    [EntityContract]
    public interface IClothComponentEntity
    {
        [PropertyContract.Required, PropertyContract.Relation.ManyToOne]
        IClothEntity Cloth { get; set; }
        [PropertyContract.Semantic.Percentage]
        decimal Percent { get; set; }
    }

    [EntityContract]
    public interface IClothEntity : IEntityPartUniqueDisplayName
    {
    }
}
