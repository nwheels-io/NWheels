using NWheels.Frameworks.Ddd;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Samples.FirstHappyPath.Domain
{
    [TransactionScriptComponent]
    public class HelloWorldTx
    {
        [TransactionScriptMethod]
        public async Task<string> Hello(string name)
        {
            await Task.Yield(); // simulate async processing
            return $"Hello world, from {name}!";
        }
    }
}
