using NUnit.Framework;

namespace NWheels.Testing
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
