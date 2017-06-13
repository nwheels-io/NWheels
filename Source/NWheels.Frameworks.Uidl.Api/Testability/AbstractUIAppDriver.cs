using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Testability
{
    public class AbstractUIAppDriver<TApp> : IAbstractUIAppDriver
        where TApp : class, IAbstractUIApp
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAbstractUIAppDriver
    {
        
    }
}
