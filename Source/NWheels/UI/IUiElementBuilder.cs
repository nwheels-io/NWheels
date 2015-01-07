using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.UI
{
    public interface IUiElementBuilder
    {
        T CreateChildBuilder<T>() where T : IUiElementBuilder;
    }
}
