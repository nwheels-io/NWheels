using System;
using System.Net.Http.Headers;
using MetaPrograms.Members;

namespace NWheels.Composition.Model.Metadata
{
    public interface IMetadataObject
    {
        MetadataObjectHeader Header { get; } 
    }
    
    public abstract class MetadataObject : IMetadataObject
    {
        private readonly MetadataObjectHeader _header;

        protected MetadataObject(MetadataObjectHeader header)
        {
            _header = header;
        }

        MetadataObjectHeader IMetadataObject.Header => _header;
    }
}
