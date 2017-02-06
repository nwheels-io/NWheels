using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace NWheels.Compilation.Adapters.Roslyn.UnitTests
{
    public class DelegateBenchmarks
    {
        public static void AStaticMethod(int x, int y)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Fact]
        public void InvokeMethodThroughDelegate()
        {
            var methodInfo = typeof(DelegateBenchmarks).GetTypeInfo().GetMethod(nameof(AStaticMethod), BindingFlags.Public | BindingFlags.Static);
            var delegateType = typeof(Action<int, int>);
            var @delegate = (Action<int, int>)methodInfo.CreateDelegate(delegateType);

            var clock = Stopwatch.StartNew();

            for (int i = 0 ; i < 1000000 ; i++)
            {
                @delegate(1, 2);
            }

            var time = clock.Elapsed;

            Assert.True(false, $"time = {time}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Fact]
        public void InvokeMethodThroughDirectClrCall()
        {
            var clock = Stopwatch.StartNew();

            for (int i = 0 ; i < 1000000 ; i++)
            {
                AStaticMethod(1, 2);
            }

            var time = clock.Elapsed;

            Assert.True(false, $"time = {time}");
        }
    }
}
