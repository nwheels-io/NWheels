using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NWheels.UI.Adapters.Web.Wix.Components;
using NWheels.UI.Model.Impl.Metadata;

namespace NWheels.UI.Adapters.Web.Wix
{
    public interface IWixComponentGenerator
    {
        WixComponentDefinition GenerateDefinition();
        string GenerateHtml();
    }

    public static class WixComponentGenerator
    {
        private static readonly Dictionary<Type, Func<UIComponentMetadata, IWixComponentGenerator>> GeneratorByCompMetaType =
            new Dictionary<Type, Func<UIComponentMetadata, IWixComponentGenerator>> {
                { typeof(TextContentMetadata), comp => new TextContentGenerator(comp) },
                { typeof(SeatingPlanMetadata), comp => new SeatingPlanGenerator(comp) },
            };

        public static IWixComponentGenerator GetGenerator(UIComponentMetadata compMeta)
        {
            var factory = GeneratorByCompMetaType[compMeta.GetType()];
            return factory(compMeta);
        }
    }
    
    public abstract class WixComponentGenerator<TMeta> : IWixComponentGenerator
        where TMeta : UIComponentMetadata
    {
        public TMeta CompMeta { get; }

        protected WixComponentGenerator(UIComponentMetadata compMeta)
        {
            CompMeta = (TMeta)compMeta;
        }
        
        public abstract WixComponentDefinition GenerateDefinition();
        public virtual string GenerateHtml() => null;
    }
}
