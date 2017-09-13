using System;
using System.Collections.Generic;

namespace NWheels.Microservices.Runtime
{
    public class AssemblyScanLoadEventArgs
    {
        public AssemblyScanLoadEventArgs(Type implementedInterface, string assemblyName)
        {
            this.AssemblyName = assemblyName;
            this.ImplementedInterface = implementedInterface;
            this.Destination = new List<Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AssemblyName { get; }
        public Type ImplementedInterface { get; }
        public List<Type> Destination { get; }
    }
}
