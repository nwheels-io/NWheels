using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Entities;
using NWheels.UI;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public class LogLevelSummaryEntityHandlerExtension : ApplicationEntityService.EntityHandlerExtension<ILogLevelSummaryEntity>
    {
        public override bool CanOpenNewUnitOfWork(object txViewModel)
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IUnitOfWork OpenNewUnitOfWork(object txViewModel)
        {
            return null;
        }
    }
}
