using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Samples.RestService
{
    public interface IOnlineStoreEntities : IApplicationEntityRepository
    {
        IQueryable<Product> Products { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<OrderLine> OrderLines { get; }
    }
}
