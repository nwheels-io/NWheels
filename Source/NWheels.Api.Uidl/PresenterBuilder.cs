using System;
using System.Linq.Expressions;
using NWheels.Api.Uidl.Specification;

namespace NWheels.Api.Uidl
{
    public class PresenterBuilder<TViewModel>
    {
        public UidlBehavior Do(Expression<Action> action)
        {
            return new UidlBehavior();
        }

        public class BehaviorBuilder
        {
            public NavigateBehaviorBuilderContext Navigate(object containerUIPart)
            {
                return new NavigateBehaviorBuilderContext();
            }
        }

        public class NavigateBehaviorBuilderContext
        {
            public NavigateBehaviorBuilderTo WithContext(Expression<Func<TViewModel, object>> selector)
            {
                return new NavigateBehaviorBuilderTo();
            }

            public UidlBehavior To(object uiPart)
            {
                return new  UidlBehavior();
            }
        }

        public class NavigateBehaviorBuilderTo
        {
            public UidlBehavior To(object uiPart)
            {
                return new  UidlBehavior();
            }
        }
    }

}