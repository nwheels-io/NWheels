using System;
using System.Collections.Generic;
using NWheels.Composition.Model.Impl.Metadata;

namespace NWheels.DevOps.Model.Impl.Metadata
{
    public class EnvironmentMetadata : MetadataObject
    {
        public EnvironmentMetadata(MetadataObjectHeader header)
            : base(header)
        {
        }

        public string Dummy { get; set; }
    }
}
