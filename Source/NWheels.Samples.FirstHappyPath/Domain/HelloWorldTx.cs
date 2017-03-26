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
        //TODO: replace task with something that does not require memory allocation per each call to TX
        [TransactionScriptMethod]
        public async Task<string> Hello(string name)
        {
            return await Task
                .Delay(10)
                .ContinueWith<string>(t => $"Hello world, from {name}!");
        }
    }
}
