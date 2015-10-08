using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel.Core
{
    internal class MetadataConventionTracker
    {
        private readonly HashSet<Type> _previewedConventionTypes;
        private readonly HashSet<Type> _appliedConventionTypes;
        private readonly HashSet<Type> _finalizedConventionTypes;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetadataConventionTracker()
        {
            _previewedConventionTypes = new HashSet<Type>();
            _appliedConventionTypes = new HashSet<Type>();
            _finalizedConventionTypes = new HashSet<Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterPreview(Type conventionType)
        {
            return _previewedConventionTypes.Add(conventionType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterApply(Type conventionType)
        {
            return _appliedConventionTypes.Add(conventionType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool RegisterFinalize(Type conventionType)
        {
            return _finalizedConventionTypes.Add(conventionType);
        }
    }
}
