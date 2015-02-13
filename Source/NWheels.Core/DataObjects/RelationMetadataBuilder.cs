using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class RelationMetadataBuilder : IRelationMetadata
    {
        #region IRelationMetadata Members

        public RelationKind RelationKind { get; set; }
        public RelationPartyKind ThisPartyKind { get; set; }
        public RelationPartyKind RelatedPartyKind { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IKeyMetadata IRelationMetadata.ThisPartyKey
        {
            get { return this.ThisPartyKey; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata IRelationMetadata.RelatedPartyType
        {
            get { return this.RelatedPartyType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IKeyMetadata IRelationMetadata.RelatedPartyKey
        {
            get { return this.RelatedPartyKey; }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public KeyMetadataBuilder ThisPartyKey { get; set; }
        public TypeMetadataBuilder RelatedPartyType { get; set; }
        public KeyMetadataBuilder RelatedPartyKey { get; set; }
    }
}
