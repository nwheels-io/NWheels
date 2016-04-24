using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Authorization
{
    public interface IEntityPropertyAccessControl
    {
        void AllowChangeAllProperties();
        void AllowChangeProperties(params Expression<Func<object>>[] properties);
    }
}
