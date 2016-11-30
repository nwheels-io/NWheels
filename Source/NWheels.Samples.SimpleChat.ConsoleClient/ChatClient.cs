using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    public class ChatClient : IChatClientApi
    {
        #region Implementation of IChatClientApi

        public void SomeoneSaidSomething(string who, string what)
        {
            Console.WriteLine("{0} > {1}", who, what);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IChatServerApi Server { get; set; }
    }
}
