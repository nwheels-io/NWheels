using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public class RelationMetadataBuilder : MetadataElement<IRelationMetadata>, IRelationMetadata
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void AcceptVisitor(IMetadataElementVisitor visitor)
        {
            RelationKind = visitor.VisitAttribute("RelationKind", RelationKind);
            ThisPartyKind = visitor.VisitAttribute("ThisPartyKind", ThisPartyKind);
            ThisPartyKey = visitor.VisitElement<IKeyMetadata, KeyMetadataBuilder>(ThisPartyKey);

            RelatedPartyType = visitor.VisitElement<ITypeMetadata, TypeMetadataBuilder>(RelatedPartyType);
            RelatedPartyKind = visitor.VisitAttribute("RelatedPartyKind", RelatedPartyKind);
            RelatedPartyKey = visitor.VisitElement<IKeyMetadata, KeyMetadataBuilder>(RelatedPartyKey);
        }
    }
}
