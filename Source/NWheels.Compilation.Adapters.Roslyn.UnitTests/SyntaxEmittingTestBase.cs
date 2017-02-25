using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public abstract class SyntaxEmittingTestBase : IDisposable
    {
        private IDisposable _tagCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected SyntaxEmittingTestBase()
        {
            _tagCache = TypeMemberTagCache.CreateOnCurrentThread();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Dispose()
        {
            _tagCache.Dispose();
        }
    }
}
