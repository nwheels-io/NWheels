using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging.Impl
{
    internal static class ThreadCpuTimeUtility
    {
        private static readonly IntPtr _s_pseudoHandle = (IntPtr)(-2);
        private static readonly ulong _s_cpuCyclesPerSecond;
        private static readonly ulong _s_cpuCyclesPerMillisecond;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static ThreadCpuTimeUtility()
        {
            long frequency;

            if ( QueryPerformanceFrequency(out frequency) )
            {
                _s_cpuCyclesPerSecond = (ulong)frequency;
                _s_cpuCyclesPerMillisecond = _s_cpuCyclesPerSecond / 1000;
            }
            else
            {
                _s_cpuCyclesPerSecond = 0;
                _s_cpuCyclesPerMillisecond = 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong GetThreadCycles()
        {
            ulong cycles;

            if ( _s_cpuCyclesPerSecond > 0 && QueryThreadCycleTime(_s_pseudoHandle, out cycles) )
            {
                return cycles;
            }

            return 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong GetThreadCpuMicroseconds(ulong startCycles, ulong endCycles)
        {
            if (_s_cpuCyclesPerMillisecond > 0)
            {
                var result = endCycles - startCycles;

                result *= 1000;
                result /= _s_cpuCyclesPerMillisecond;

                return result;
            }
            else
            {
                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong GetThreadCpuMicroseconds(ulong usedCycles)
        {
            if (_s_cpuCyclesPerMillisecond > 0)
            {
                var result = usedCycles;

                result *= 1000;
                result /= _s_cpuCyclesPerMillisecond;

                return result;
            }
            else
            {
                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong GetThreadCpuMilliseconds(ulong startCycles, ulong endCycles)
        {
            if (_s_cpuCyclesPerSecond > 0)
            {
                var result = endCycles - startCycles;

                result *= 1000;
                result /= _s_cpuCyclesPerSecond;

                return result;
            }
            else
            {
                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong GetThreadCpuMilliseconds(ulong usedCycles)
        {
            if (_s_cpuCyclesPerSecond > 0)
            {
                var result = usedCycles;

                result *= 1000;
                result /= _s_cpuCyclesPerSecond;

                return result;
            }
            else
            {
                return 0;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ulong CpuCyclesPerSecond
        {
            get
            {
                return _s_cpuCyclesPerSecond;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsThreadCpuTimeSupported
        {
            get
            {
                return (_s_cpuCyclesPerSecond > 0);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryThreadCycleTime(IntPtr hThread, out ulong cycles);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long frequency);
    }
}
