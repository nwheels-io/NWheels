using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NWheels.Frameworks.Uidl.Abstractions
{
    public abstract class AbstractUIElement<TViewModel> : IAbstractUIElement
    {
        public TElement Element<TElement>(Expression<Func<TViewModel, object>> pathInModel)
            where TElement : IAbstractUIElement
        {
            return default(TElement);
        }

        public IAbstractUIElement Element(Expression<Func<TViewModel, object>> pathInModel)
        {
            return null;
        }

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
