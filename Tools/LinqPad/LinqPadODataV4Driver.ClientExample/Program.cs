using LinqPadODataV4Driver.ClientExample.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPadODataV4Driver.ClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container(new Uri("http://localhost:9000/entity"));

            container.SendingRequest2 += (sender, e) => Console.WriteLine(e.RequestMessage.Url);

            var allProducts = container.Product.ToArray();

            foreach (var product in allProducts)
            {
                Console.WriteLine("Product ID={0}, Name={1}, Price={2}", product.Id, product.Name, product.Price);
            }
        }
    }
}
