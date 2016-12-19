using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Concurrency;
using NWheels.Processing.Commands;
using Shouldly;

namespace NWheels.Testing.Extensions
{
    public static class ShouldExtensions
    {
        public static void ShouldHaveNoErrors(this IMethodCallObject call)
        {
            ((IAnyDeferred)call).Error.ShouldBeNull();

            var resultTask = call.Result as Task;

            if (resultTask != null)
            {
                resultTask.Exception.ShouldBeNull();
            }

            var resultPromise = call.Result as IAnyPromise;

            if (resultPromise != null)
            {
                resultPromise.Error.ShouldBeNull();
            }
        }
    }
}
