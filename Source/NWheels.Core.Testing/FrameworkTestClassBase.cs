using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NWheels.Core.Testing
{
    [TestFixture]
    public abstract class FrameworkTestClassBase
    {
        protected T Resolve<T>() where T : class
        {
            return null;
        }
    }
}
