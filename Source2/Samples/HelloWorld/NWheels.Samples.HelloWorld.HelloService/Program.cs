using NWheels.Microservices;
using System;

namespace NWheels.Samples.HelloWorld.HelloService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var m = new ClassFromMicroservices();
            Console.WriteLine(m.M());
        }
    }
}
