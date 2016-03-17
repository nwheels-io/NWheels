using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.TypeModel;

namespace NWheels.UI.Impl
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityExportFormatSemanticsAttribute : PropertyContract.Semantic.SemanticAttributeBase
    {
        public EntityExportFormatSemanticsAttribute()
            : base(typeof(string), WellKnownSemanticType.None, defaultConfiguration: null)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of SemanticAttributeBase

        protected override void OnConfigurePropertySemantics(
            TypeMetadataCache cache,
            PropertyMetadataBuilder property,
            SemanticType.SemanticDataTypeBuilder semantics)
        {
            var formats = cache.Components.Resolve<IEnumerable<IEntityExportFormat>>();
            semantics.StandardValues = formats.Select(f => f.FormatTitle).Cast<object>().ToArray();
        }

        #endregion
    }
}
