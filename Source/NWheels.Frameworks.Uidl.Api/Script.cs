using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Uidl
{
    public static class Script
    {
        public static PromiseBuilder Call<T>(Func<T, Task> call)
        {
            return new PromiseBuilder();
        }

        public static PromiseBuilder Assign(object target, object value)
        {
            return new PromiseBuilder();
        }

        public static PromiseBuilder Alert(AlertType type, string format, params object[] args)
        {
            return new PromiseBuilder();
        }
    }

    public enum AlertType
    {
        None,
        Info,
        Success,
        Warning,
        Error
    }
}
