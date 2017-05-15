using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Generic
{
    public class CompositeUIElement : AbstractUIElement<Empty.ViewModel>, ICompositeUIElement
    {
        public void Add(IAbstractUIElement element)
        {
        }
    }

    public interface ICompositeUIElement
    {
    }
}
