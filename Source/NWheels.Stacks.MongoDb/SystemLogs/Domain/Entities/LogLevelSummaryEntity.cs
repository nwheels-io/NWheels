using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Entities;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.SystemLogs.Persistence;
using NWheels.UI;

namespace NWheels.Stacks.MongoDb.SystemLogs.Domain.Entities
{
    public abstract class LogLevelSummaryEntity : ILogLevelSummaryEntity
    {
        public void SetKey(string environment, string machine, string node, string instance, string replica)
        {
            this.Environment = environment;
            this.Machine = machine;
            this.Node = node;
            this.Instance = instance;
            this.Replica = replica;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Increment(LogLevel level, int count)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    this.DebugCount += count;
                    break;
                case LogLevel.Verbose:
                    this.VerboseCount += count;
                    break;
                case LogLevel.Info:
                    this.InfoCount += count;
                    break;
                case LogLevel.Warning:
                    this.WarningCount += count;
                    break;
                case LogLevel.Error:
                    this.ErrorCount += count;
                    break;
                case LogLevel.Critical:
                    this.CriticalCount += count;
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string Id { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Machine { get; private set; }
        public string Environment { get; private set; }
        public string Node { get; private set; }
        public string Instance { get; private set; }
        public string Replica { get; private set; }
        public int DebugCount { get; private set; }
        public int VerboseCount { get; private set; }
        public int InfoCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }
        public int CriticalCount { get; private set; }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HandlerExtension : ApplicationEntityService.EntityHandlerExtension<ILogLevelSummaryEntity>
        {
            public override bool CanOpenNewUnitOfWork(object txViewModel)
            {
                return true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IUnitOfWork OpenNewUnitOfWork(object txViewModel)
            {
                return null;
            }

            ////-----------------------------------------------------------------------------------------------------------------------------------------------

            //public override ApplicationEntityService.EntityHandler CreateEntityHandler(
            //    ApplicationEntityService owner, 
            //    ITypeMetadata metaType, 
            //    Type domainContextType, 
            //    ApplicationEntityService.IEntityHandlerExtension[] extensions)
            //{
            //    return new CustomEntityHandler(owner, metaType, domainContextType, extensions);
            //}

            ////-----------------------------------------------------------------------------------------------------------------------------------------------

            //public override bool CanCreateEntityHandler
            //{
            //    get { return true; }
            //}


            ////-----------------------------------------------------------------------------------------------------------------------------------------------

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

            //    //-------------------------------------------------------------------------------------------------------------------------------------------

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
}
