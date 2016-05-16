using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
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

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public override ApplicationEntityService.EntityHandler CreateEntityHandler(
        //    ApplicationEntityService owner, 
        //    ITypeMetadata metaType, 
        //    Type domainContextType, 
        //    ApplicationEntityService.IEntityHandlerExtension[] extensions)
        //{
        //    return new CustomEntityHandler(owner, metaType, domainContextType, extensions);
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public override bool CanCreateEntityHandler
        //{
        //    get { return true; }
        //}


        ////------------------------------------------------------------------------------------------------------------------------------------------------------

        //public class CustomEntityHandler : ApplicationEntityService.EntityHandler<ISystemLogContext, ILogLevelSummaryEntity>
        //{
        //    public CustomEntityHandler(
        //        ApplicationEntityService owner, 
        //        ITypeMetadata metaType, 
        //        Type domainContextType, 
        //        ApplicationEntityService.IEntityHandlerExtension[] extensions)
        //        : base(owner, metaType, domainContextType, extensions)
        //    {
        //    }

        //    //-------------------------------------------------------------------------------------------------------------------------------------------------

        //    public override ApplicationEntityService.QueryResults Query(
        //        ApplicationEntityService.QueryOptions options, 
        //        IQueryable query = null, 
        //        object txViewModel = null)
        //    {
        //        var results = ApplicationEntityService.QueryContext.Current.Results;

        //        results.ResultSet = query.Cast<object>().ToArray();

        //        if (options.Page.HasValue)
        //        {
        //            results.PageNumber = options.Page.Value;
        //            results.
        //        }
        //    }
        //}

    }
}
