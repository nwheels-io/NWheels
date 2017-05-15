using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Frameworks.Uidl.Abstractions
{
    public abstract class AbstractUIElement<TViewModel> : IAbstractUIElement
    {
        public TViewModel Model => default(TViewModel);

        protected virtual void ConfigureElements()
        {
        }
        protected virtual void ImplementController()
        {
        }
    }

    public interface IAbstractUIElement
    {
    }
}
