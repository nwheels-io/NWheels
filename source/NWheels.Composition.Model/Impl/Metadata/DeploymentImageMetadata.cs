using System.Collections.Generic;
using System.IO;
using MetaPrograms;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public class DeploymentImageMetadata : MetadataObject
    {
        public DeploymentImageMetadata(ITechnologyAdapterContext context) 
            : base(MetadataObjectHeader.NoSourceType())
        {
        }
        
        public string Name { get; set; }
        public string BaseImage { get; set; }
        public FilePath BuildContextPath { get; set; }
        public Dictionary<FilePath, FilePath> FilesToCopy { get; } = new Dictionary<FilePath, FilePath>();
        public string[] EntryPointCommand { get; set; }
    }
}
