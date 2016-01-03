using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NWheels.UnitTests.UI
{
    [TestFixture]
    public class ApplicationEntityServiceTests
    {
        [Test]
        public void CanAggrageteMultiplePropertiesWithLinq()
        {
            var recordList = new Record[] {
                new Record() { X = 10, Y = 20, Z = 30 },
                new Record() { X = 11, Y = 21, Z = 31 },
                new Record() { X = 12, Y = 22, Z = 32 },
            };

            var recordListQueryable = recordList.AsQueryable();

            var aggregationQuery = recordListQueryable
                .GroupBy(r => 1)
                .Select(g => new {
                    X = g.Sum(r => r.X),
                    Y = g.Min(r => r.Y),
                    Z = g.Max(r => r.Z)
                });

            var aggregationResult = aggregationQuery.Single();

            Console.WriteLine(JsonConvert.SerializeObject(aggregationResult));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Record
        {
            public int X { get; set; }
            public float Y { get; set; }
            public decimal Z { get; set; }
        }
    }
}
