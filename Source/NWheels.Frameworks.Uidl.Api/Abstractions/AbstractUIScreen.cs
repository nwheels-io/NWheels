using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Abstractions
{
    public abstract class AbstractUIScreen<TViewModel> : AbstractUIElement<TViewModel>, IAbstractUIScreen
    {
    }

    public interface IAbstractUIScreen
    {
    }
}
