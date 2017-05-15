using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NWheels.Frameworks.Uidl.Abstractions
{
    public class PromiseBuilder
    {
        public PromiseBuilder Then(params Expression<Func<object, PromiseBuilder>>[] codeBlock)
        {
            return new PromiseBuilder();
        }

        public PromiseBuilder Fail(params Expression<Func<object, PromiseBuilder>>[] codeBlock)
        {
            return new PromiseBuilder();
        }
    }
}
