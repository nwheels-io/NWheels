using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.SystemMonitor.Contracts
{
    [EntityContract]
    public interface IKeyValueIndexEntity
    {
        string Key { get; set; }
        string Value { get; set; }
    }
}
