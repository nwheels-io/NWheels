using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security;
using NWheels.Entities;

namespace NWheels.Samples.MyHRApp.Domain
{
    [EntityContract]
    public interface IHRUserAccountEntity : IUserAccountEntity, IEntityPartUserAccountProfilePhoto
    {
    }
}
