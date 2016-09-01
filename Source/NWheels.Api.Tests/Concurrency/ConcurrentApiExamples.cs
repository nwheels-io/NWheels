using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Api.Concurrency;

namespace NWheels.Api.Tests.Concurrency
{
    public class ConcurrentApiExamples
    {
        public void DeferAndReceiveAnyExample()
        {
            IFramework framework = null;

            var channel1 = framework.Scheduler.NewChannel<int>("c1");
            var channel2 = framework.Scheduler.NewChannel<string>("c2");
            var quit = framework.Scheduler.NewChannel<bool>("q");

            framework.Scheduler.Defer(async () => {
                while (true)
                {
                    object value; 
                    switch (await framework.Scheduler.TryReceiveAnyAsync(TimeSpan.FromSeconds(3), out value, channel1, channel2, quit))
                    {
                        case 0:
                            Console.WriteLine($"received {value} <-c1");
                            break;
                        case 1:
                            Console.WriteLine($"received {value} <-c2");
                            break;
                        case 2:
                            Console.WriteLine("received quit");
                            return;
                        default:
                            Console.WriteLine("timed out!");
                            return;
                    }
                }
            });

            for (int i = 0; i < 10 ; i++)
            {
                channel1.Producer.Send(i);
                channel2.Producer.Send((i * 2).ToString());
                Thread.Sleep(100);
            }

            quit.Producer.Send(true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async void AsyncAwaitOnPromiseExample()
        {
            IFramework framework = null;

            int x = await framework.Scheduler.Defer<int>(() => {
                return 123;
            });

            Console.WriteLine(x.ToString());            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        // public void PromiseExample()
        // {
        //     IFramework framework = null;

        //     var channel1 = framework. MakeChannel<int>();
        //     var channel2 = framework.MakeChannel<string>();
        //     var quit = framework.MakeChannel<bool>();

        //     framework.Defer(() => {
        //         var m = new MemoryStream();
        //         return m;
        //     }).Then(
        //         success: m => m.Length,
        //         failure: err => Console.WriteLine(err.ToString())
        //     );
        // }
    }
}