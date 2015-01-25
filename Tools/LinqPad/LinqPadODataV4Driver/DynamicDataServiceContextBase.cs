using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Xml;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace LinqPadODataV4Driver
{
    public abstract class DynamicDataServiceContextBase : DataServiceContext
    {
        private readonly string _localEntityTypesNamespace;
        private string _remoteEntityTypesNamespace;
        private IEdmModel _edmModel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DynamicDataServiceContextBase(Uri serviceRoot, string localEntityTypesNamespace) : 
            base(serviceRoot, ODataProtocolVersion.V4)
        {
            _localEntityTypesNamespace = localEntityTypesNamespace;

            this.ResolveName = ResolveNameFromType;
            this.ResolveType = ResolveTypeFromName;

            this.Format.LoadServiceModel = LoadServiceModel;
            this.Format.UseJson();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type ResolveTypeFromName(string typeName)
        {
            Type resolvedType = this.DefaultResolveType(typeName, _remoteEntityTypesNamespace, _localEntityTypesNamespace);

            if ( (resolvedType != null) )
            {
                return resolvedType;
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ResolveNameFromType(Type clientType)
        {
            var originalNameAttribute = clientType.GetCustomAttributes(typeof(OriginalNameAttribute), true).OfType<OriginalNameAttribute>().SingleOrDefault();

            if ( clientType.Namespace == _localEntityTypesNamespace )
            {
                if ( originalNameAttribute != null )
                {
                    return string.Concat(_remoteEntityTypesNamespace + ".", originalNameAttribute.OriginalName);
                }
                return string.Concat(_remoteEntityTypesNamespace + ".", clientType.Name);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEdmModel LoadServiceModel()
        {
            _edmModel = LoadModelFromService(this.GetMetadataUri());
            _remoteEntityTypesNamespace = _edmModel.GetEntityNamespace();

            return _edmModel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEdmModel LoadModelFromService(Uri metadataUri)
        {
            using ( var http = new WebClient() )
            {
                var metadataString = http.DownloadString(metadataUri);

                using ( var xmlReader = XmlReader.Create(new StringReader(metadataString)) )
                {
                    return EdmxReader.Parse(xmlReader);
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object CreateObjectByTypeName(string fullTypeName)
        {
            var type = Type.GetType(fullTypeName);
            return Activator.CreateInstance(type, null, TrackingMode.None);
        }
    }
}