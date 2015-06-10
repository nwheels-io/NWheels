using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hapil;

namespace NWheels.Endpoints
{
    public abstract class AbstractEndpointRegistration
    {
        private Uri _address;
        private Uri _metadataAddress;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractEndpointRegistration(
            string name, 
            Type contract, 
            string defaultListenUrl, 
            bool publishMetadata, 
            string defaultMetadataUrl, 
            bool exposeExceptionDatails)
        {
            this.EndpointIndexInNodeInstance = Interlocked.Increment(ref _s_nextEndpointIndex);
            this.Name = (string.IsNullOrWhiteSpace(name) ? contract.Name : name);
            this.Contract = contract;
            this.Address = (string.IsNullOrWhiteSpace(defaultListenUrl) ? null : new Uri(defaultListenUrl));
            this.ShouldPublishMetadata = publishMetadata;
            this.ShouldExposeExceptionDetails = exposeExceptionDatails;
            this.MetadataAddress = (string.IsNullOrWhiteSpace(defaultMetadataUrl) ? null : new Uri(defaultMetadataUrl));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Uri GetDefaultAddress()
        {
            if ( !string.IsNullOrWhiteSpace(this.Name) )
            {
                //return new Uri(string.Format("http://localhost:8900/", this.Name.TrimPrefix("I")));
                return new Uri(string.Format("http://localhost:8900/{0}", this.Name.TrimPrefix("I")));
            }
            else
            {
                return new Uri(string.Format("http://localhost:{0}", 8900 + this.EndpointIndexInNodeInstance));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Uri GetDefaultMetadataAddress()
        {
            return new Uri(Address.ToString().EndsWith("/") ? Address : new Uri(Address.ToString() + "/"), "metadata");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool ApplyConfiguration(IFrameworkEndpointsConfig configSection)
        {
            IEndpointConfig endpointElement;

            if ( configSection.Endpoints.TryGetElementByName(this.Name, out endpointElement) )
            {
                if ( !string.IsNullOrEmpty(endpointElement.Address) )
                {
                    this.Address = new Uri(endpointElement.Address);
                }
                
                if ( !string.IsNullOrEmpty(endpointElement.MetadataAddress) )
                {
                    this.Address = new Uri(endpointElement.MetadataAddress);
                }

                this.ShouldPublishMetadata = endpointElement.PuiblishMetadata;
                this.ShouldExposeExceptionDetails = endpointElement.ExposeExceptionDetails;
                
                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public Type Contract { get; set; }
        public bool ShouldPublishMetadata { get; set; }
        public bool ShouldExposeExceptionDetails { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Uri Address
        {
            get
            {
                if ( _address == null )
                {
                    _address = GetDefaultAddress();
                }

                return _address;
            }
            set
            {
                _address = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Uri MetadataAddress
        {
            get
            {
                if ( _metadataAddress == null )
                {
                    _metadataAddress = GetDefaultMetadataAddress();
                }

                return _metadataAddress;
            }
            set
            {
                _metadataAddress = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int EndpointIndexInNodeInstance { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int _s_nextEndpointIndex = 0;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class JsonApiEndpointRegistration : AbstractEndpointRegistration
    {
        public JsonApiEndpointRegistration(
            string name, Type contract, string defaultListenUrl, string defaultMetadataUrl, bool publishMetadata, bool exposeExceptions)
            : base(name, contract, defaultListenUrl, publishMetadata, defaultMetadataUrl, exposeExceptions)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SoapApiEndpointRegistration : AbstractEndpointRegistration
    {
        public SoapApiEndpointRegistration(
            string name, Type contract, string defaultListenUrl, string defaultMetadataUrl, bool publishMetadata, bool exposeExceptions)
            : base(name, contract, defaultListenUrl, publishMetadata, defaultMetadataUrl, exposeExceptions)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class RestApiEndpointRegistration : AbstractEndpointRegistration
    {
        public RestApiEndpointRegistration(
            string name, Type contract, string defaultListenUrl, string defaultMetadataUrl, bool publishMetadata, bool exposeExceptions)
            : base(name, contract, defaultListenUrl, publishMetadata, defaultMetadataUrl, exposeExceptions)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WebAppEndpointRegistration : AbstractEndpointRegistration
    {
        public WebAppEndpointRegistration(
            string name, Type contract, Type codeBehindType, string defaultUrl, bool exposeExceptions)
            : base(name, contract, defaultUrl, publishMetadata: false, defaultMetadataUrl: null, exposeExceptionDatails: exposeExceptions)
        {
            this.CodeBehindType = codeBehindType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type CodeBehindType { get; private set; }
    }
}
