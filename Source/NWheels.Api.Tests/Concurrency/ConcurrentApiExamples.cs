using System;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Api.Concurrency;

namespace NWheels.Api.Tests.Concurrency
{
    public class ConcurrentApiExamples
    {
        public void GoAndSelectExample()
        {
            IFramework framework = null;

            var channel1 = framework.MakeChannel<int>();
            var channel2 = framework.MakeChannel<string>();
            var quit = framework.MakeChannel<bool>();

            framework.Go(() => {
                while (true)
                {
                    SelectResult result; 
                    
                    switch (framework.Select(out result, channel1, channel2, quit))
                    {
                        case 0:
                            Console.WriteLine($"received {result.As<int>()} <-0");
                            break;
                        case 1:
                            Console.WriteLine($"received {result.As<string>()} <-1");
                            break;
                        case 3:
                            return;
                    }
                }
            });

            for (int i = 0; i < 10 ; i++)
            {
                channel1.Producer.Send(i);
                channel2.Producer.Send("AAA");
            }
            Thread.Sleep(100);
            channel1.Producer.Send(111);
            Thread.Sleep(500);
            channel2.Producer.Send("AAA");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void PromiseExample()
        {

        }
    }
}