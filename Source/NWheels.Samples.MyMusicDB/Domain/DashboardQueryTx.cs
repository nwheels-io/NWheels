using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.DataObjects;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.Domains.Security;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;

namespace NWheels.Samples.MyMusicDB.Domain
{
    [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
    public class DashboardQueryTx : ITransactionScript<Empty.Context, Empty.Input, DashboardQueryTx.IOutput>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly AbstractLogMessageSummaryTx _logSummaryTx;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DashboardQueryTx(IFramework framework, IViewModelObjectFactory viewModelFactory, AbstractLogMessageSummaryTx logSummaryTx)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _logSummaryTx = logSummaryTx;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of ITransactionScript<Context,Input,Result>

        public Empty.Input InitializeInput(Empty.Context context)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Preview(Empty.Input input)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IOutput Execute(Empty.Input input)
        {
            var output = _viewModelFactory.NewEntity<IOutput>();

            QueryApiRequestsOverLast48Hours(output);
            QueryTotals(output);

            return output;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void QueryTotals(IOutput output)
        {
            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                long apiRequests;
                long uniqueUsers;
                context.QueryEventCounters(out apiRequests, out uniqueUsers);

                output.TotalApiRequests = apiRequests;
                output.Users = uniqueUsers;
                output.Genres = context.Genres.AsQueryable().Count();
                output.Artists = context.Artists.AsQueryable().Count();
                output.Albums = context.Albums.AsQueryable().Count();
                output.Tracks = context.Tracks.AsQueryable().Count();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void QueryApiRequestsOverLast48Hours(IOutput output)
        {
            var now = _framework.UtcNow;

            var logSummaryInput = _viewModelFactory.NewEntity<ILogTimeRangeCriteria>();
            logSummaryInput.From = now.AddHours(-48);
            logSummaryInput.Until = now;
            logSummaryInput.SeriesIndex = 0;

            var upstreamUiContext = UIOperationContext.Current;
            var logSummaryQuery = new ApplicationEntityService.QueryOptions(
                upstreamUiContext.EntityName,
                new Dictionary<string, string> {
                    { "MessageId", "ApiRequest.ApiRequestProcessed" }
                });

            using (new UIOperationContext(upstreamUiContext, logSummaryQuery))
            {
                var logSummaryOutput = _logSummaryTx.Execute(logSummaryInput).As<VisualizedQueryable<ILogMessageSummaryEntity>>();
                output.ApiRequestsOverTime = logSummaryOutput.Visualization;
                output.ApiRequestsOverTime.Series[0].Label = "Requests";
                var summaryRecord = logSummaryOutput.FirstOrDefault();

                if (summaryRecord != null)
                {
                    output.ApiRequestsInLast48Hours = summaryRecord.Count;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            int Genres { get; set; }
            int Artists { get; set; }
            int Albums { get; set; }
            int Tracks { get; set; }
            long Users { get; set; }
            long TotalApiRequests { get; set; }
            int ApiRequestsInLast48Hours { get; set; }
            ChartData ApiRequestsOverTime { get; set; }
        }
    }
}