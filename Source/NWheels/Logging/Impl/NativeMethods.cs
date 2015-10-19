using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging.Impl
{
    internal static class NativeMethods
    {
        public static ulong GetThreadCycles()
        {
            ulong cycles;

            if ( !QueryThreadCycleTime(_s_pseudoHandle, out cycles) )
            {
                throw new System.ComponentModel.Win32Exception();
            }

            return cycles;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryThreadCycleTime(IntPtr hThread, out ulong cycles);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long frequency);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly IntPtr _s_pseudoHandle = (IntPtr)(-2);
        private static readonly ulong _s_cpuCyclesPerMillisecond;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static NativeMethods()
        {
            long frequency;

            if ( QueryPerformanceFrequency(out frequency) )
            {
                _s_cpuCyclesPerMillisecond = (ulong)frequency / 1000;
            }
            else
            {
                _s_cpuCyclesPerMillisecond = 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong CpuCyclesPerMillisecond
        {
            get
            {
                return _s_cpuCyclesPerMillisecond;
            }
        }
    }
}
