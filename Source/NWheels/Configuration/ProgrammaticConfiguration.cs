using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Configuration
{
    public class ProgrammaticConfiguration
    {
        private readonly Action<IComponentContext> _apply;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProgrammaticConfiguration(Action<IComponentContext> apply)
        {
            _apply = apply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void Apply(IComponentContext components)
        {
            _apply(components);
        }
    }
}
