using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Generic
{
    public class DockLayoutElement : AbstractUIElement<Empty.ViewModel>
    {
        public CompositeUIElement Left { get; }
        public CompositeUIElement Right { get; }
        public CompositeUIElement Top { get; }
        public CompositeUIElement Bottom { get; }
        public CompositeUIElement Fill { get; }
    }
}
