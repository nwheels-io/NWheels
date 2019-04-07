using System;

namespace NWheels.Composition.Model.Impl.Metadata
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ProgrammingModelAttribute : Attribute
    {
        public ProgrammingModelAttribute(Type entryPointClass)
        {
            this.EntryPointClass = entryPointClass;
        }

        public Type EntryPointClass { get; }
    }
}