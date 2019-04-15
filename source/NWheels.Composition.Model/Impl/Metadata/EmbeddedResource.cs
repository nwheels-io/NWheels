using System;
using System.IO;
using System.Reflection;

namespace NWheels.Composition.Model.Impl.Metadata
{
    public static class EmbeddedResource
    {
        public static string LoadAsString(string pathWithinAssembly)
        {
            var assembly = Assembly.GetCallingAssembly();
            var fullResourcePath = $"{assembly.GetName().Name}.{pathWithinAssembly}";
            var resource = assembly.GetManifestResourceStream(fullResourcePath);

            if (resource == null)
            {
                throw new ArgumentException(
                    $"Embedded resource not found: {fullResourcePath}", 
                    nameof(pathWithinAssembly));
            }

            var contents = new StreamReader(resource).ReadToEnd();
            return contents;
        }
    }
}
