using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.Kernel.Api.Execution;
using NWheels.RestApi.Api;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloTxHelloMethodResourceHandler : IResourceHandler<HelloTxHelloMethodInvocation>
    {
        private static readonly string _s_fullPath = "tx/Hello/Hello";
        private static readonly string _s_classifierPath = "tx/Hello";
        private static readonly string _s_description = "Transaction script 'Hello'";

        private readonly Func<Program.HelloTx> _txFactory;

        public HelloTxHelloMethodResourceHandler(Func<Program.HelloTx> txFactory)
        {
            _txFactory = txFactory;
        }
        
        public async Task PostNew(HelloTxHelloMethodInvocation data)
        {
            var tx = _txFactory();
            await data.Invoke(tx);
        }

        Task<IEnumerable<HelloTxHelloMethodInvocation>> IResourceHandler<HelloTxHelloMethodInvocation>.GetByQuery(IResourceQuery query) =>                
            throw new NotSupportedException();

        Task IResourceHandler<HelloTxHelloMethodInvocation>.PatchByQuery(IResourceQuery query, HelloTxHelloMethodInvocation patch) =>
            throw new NotSupportedException();
                
        Task IResourceHandler<HelloTxHelloMethodInvocation>.DeleteByQuery(IResourceQuery query) =>
            throw new NotSupportedException();

        string IResourceHandler.FullPath => _s_fullPath;
        string IResourceHandler.ClassifierPath => _s_classifierPath;
        string IResourceHandler.Description => _s_description;
        Type IResourceHandler.KeyType => null;
        Type IResourceHandler.DataType => typeof(HelloTxHelloMethodInvocation);

        bool IResourceHandler.CanGetById => false;
        bool IResourceHandler.CanGetByQuery => false;
        bool IResourceHandler.CanPostNew => true;
        bool IResourceHandler.CanPostById => false;
        bool IResourceHandler.CanPatchById => false;
        bool IResourceHandler.CanPatchByQuery => false;
        bool IResourceHandler.CanDeleteById => false;
        bool IResourceHandler.CanDeleteByQuery => false;
    }
}