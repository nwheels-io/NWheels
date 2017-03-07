using System;
using System.Collections.Generic;

namespace NWheels.Microservices
{
    public class AssemblyLoadEventArgs
    {
        public AssemblyLoadEventArgs(Type implementedInterface, string directoryPath, string assemblyName)
        {
            this.AssemblyName = assemblyName;
            this.DirectoryPath = directoryPath;
            this.ImplementedInterface = implementedInterface;
            this.Destination = new List<Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string AssemblyName { get; }
        public string DirectoryPath { get; }
        public Type ImplementedInterface { get; }
        public List<Type> Destination { get; }
    }
}
