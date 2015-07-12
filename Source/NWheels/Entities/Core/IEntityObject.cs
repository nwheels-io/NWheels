using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Entities.Core
{
    public interface IEntityObject
    {
        IEntityId GetId();
        void SetId(object value);
        Type ContractType { get; }
    }
}
