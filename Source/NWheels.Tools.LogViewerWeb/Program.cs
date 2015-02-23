using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;

namespace NWheels.Tools.LogViewerWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://+:8999";

            using ( WebApp.Start<Startup>(url) )
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseNancy();
            }
        }
    }
}
