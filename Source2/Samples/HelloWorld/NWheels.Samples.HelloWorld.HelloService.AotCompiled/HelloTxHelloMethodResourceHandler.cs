using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.Kernel.Api.Execution;
using NWheels.RestApi.Api;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloTxHelloMethodResourceDescription : IResourceDescription
    {
        internal static readonly string _s_uriPath = "/api/tx/Hello/Hello";
        internal static readonly string _s_classifierUriPath = "/api/tx/Hello";
        internal static readonly string _s_description = "HelloTx component";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string UriPath => _s_uriPath;
        string IResourceDescription.ClassifierUriPath => _s_classifierUriPath;
        string IResourceDescription.Description => _s_description;
        Type IResourceDescription.KeyType => null;
        Type IResourceDescription.DataType => typeof(HelloTxHelloMethodInvocation);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        bool IResourceDescription.CanGetById => false;
        bool IResourceDescription.CanGetByQuery => false;
        bool IResourceDescription.CanPostNew => true;
        bool IResourceDescription.CanPostById => false;
        bool IResourceDescription.CanPatchById => false;
        bool IResourceDescription.CanPatchByQuery => false;
        bool IResourceDescription.CanDeleteById => false;
        bool IResourceDescription.CanDeleteByQuery => false;
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloTxHelloMethodResourceHandler : HelloTxHelloMethodResourceDescription, IResourceHandler<HelloTxHelloMethodInvocation>
    {
        private readonly Func<Program.HelloTx> _txFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public HelloTxHelloMethodResourceHandler(Func<Program.HelloTx> txFactory)
        {
            _txFactory = txFactory;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public async Task PostNew(HelloTxHelloMethodInvocation data)
        {
            var tx = _txFactory();
            await data.Invoke(tx);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        Task<IEnumerable<HelloTxHelloMethodInvocation>> IResourceHandler<HelloTxHelloMethodInvocation>.GetByQuery(IResourceQuery query) =>                
            throw new NotSupportedException();

        Task IResourceHandler<HelloTxHelloMethodInvocation>.PatchByQuery(IResourceQuery query, HelloTxHelloMethodInvocation patch) =>
            throw new NotSupportedException();
                
        Task IResourceHandler<HelloTxHelloMethodInvocation>.DeleteByQuery(IResourceQuery query) =>
            throw new NotSupportedException();
    }
}
