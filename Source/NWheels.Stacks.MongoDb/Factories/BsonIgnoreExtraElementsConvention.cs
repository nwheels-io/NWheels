using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Writers;
using MongoDB.Bson.Serialization.Attributes;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonIgnoreExtraElementsConvention : DecorationConvention
    {
        public BsonIgnoreExtraElementsConvention()
            : base(Will.DecorateClass)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DecorationConvention

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            classWriter.Attribute<BsonIgnoreExtraElementsAttribute>();
        }

        #endregion
    }
}
