using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation.Mechanism.Factories
{
    public interface ITypeFactoryConvention
    {
        bool ShouldApply(ITypeFactoryContext context);
        void Validate(ITypeFactoryContext context);
        void Declare(ITypeFactoryContext context);
        void Implement(ITypeFactoryContext context);
    }
}
