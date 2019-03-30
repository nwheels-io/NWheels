using System;

namespace NWheels.Composition.Model.Parsers
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