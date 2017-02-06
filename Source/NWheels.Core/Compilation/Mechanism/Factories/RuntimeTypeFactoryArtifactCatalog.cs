using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public abstract class RuntimeTypeFactoryArtifactCatalog
    {
        public const string ConcreteCatalogClassName = "ThisAssemblyArtifactCatalog";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract RuntimeTypeFactoryArtifact[] GetArtifacts();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static RuntimeTypeFactoryArtifactCatalog LoadFrom(Assembly assembly)
        {
            var catalogType = assembly.GetType(ConcreteCatalogClassName, throwOnError: true);
            var catalogInstance = (RuntimeTypeFactoryArtifactCatalog)Activator.CreateInstance(catalogType);
            return catalogInstance;
        }
    }
}
