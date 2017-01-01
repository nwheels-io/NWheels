using NWheels.Mechanism.Concurrency;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Mechanism.IoC
{
    public interface IInjector
    {
        T Acquire<T>();
        T Acquire<T, TArg1>(TArg1 arg1);
        T Acquire<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2);
        T Acquire<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        bool TryGet<T>(out T component);
        bool TryGet<T, TArg1>(TArg1 arg1, out T component);
        bool TryGet<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, out T component);
        bool TryGet<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, out T component);

        T GetByKey<T>(string key);
        T GetByKey<T, TArg1>(string key, TArg1 arg1);
        T GetByKey<T, TArg1, TArg2>(string key, TArg1 arg1, TArg2 arg2);
        T GetByKey<T, TArg1, TArg2, TArg3>(string key, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        bool GetByKey<T>(string key, out T component);
        bool GetByKey<T, TArg1>(string key, TArg1 arg1, out T component);
        bool GetByKey<T, TArg1, TArg2>(string key, TArg1 arg1, TArg2 arg2, out T component);
        bool GetByKey<T, TArg1, TArg2, TArg3>(string key, TArg1 arg1, TArg2 arg2, TArg3 arg3, out T component);

        Task<bool> TryGetAsync<T>(out T component);

        IEnumerable<T> GetAll<T>();
        IEnumerable<T> GetAllByKey<T>(string key);

        IPipeline<T> GetPipeline<T>();
        IPipeline<T> GetPipelineByKey<T>(string key);
    }

    public class InjectorApiTest
    {
        private DateTime _ts = DateTime.Now;

        public static void M2(out int x)
        {
            x = 123;
        }

        public void M3()
        {
            var s = "ABC";
            M2(out int x);
            L2(L1(x));

            int L1(int z)
            {
                return z * 2;
            }

            void L2(int z)
            {
                Console.WriteLine("{0} : {1} : {2}", z, s, _ts);
            }
        }

        public static async Task<string> M1Async(IInjector injector)
        {
            HostProgram program;
            if (await injector.TryGetAsync(out program))
            {
                return "OK";
            }

            return "fail";


        } 
    }   
}
