using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Domains.DevOps.Logging.Entities;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.UI.ScreenParts
{
    public class LogLevelSummaryScreenPart : ScreenPartBase<LogLevelSummaryScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public LogLevelSummaryScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<LogLevelSummaryScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Report;

            presenter.On(base.NavigatedHere)
                .Broadcast(Report.ContextSetter).WithPayload(vm => vm.Input).TunnelDown();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChartTableReport<Empty.Context, ISystemLogCriteria, ChartTx, MessageTx, ILogMessageEntity> Report { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RegisterComponents(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<ChartTx>();
            builder.NWheelsFeatures().Processing().RegisterTransactionScript<MessageTx>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface ISystemLogCriteria
        {
            DateTime From { get; set; }
            DateTime Until { get; set; }
            string Node { get; set; }
            string Instance { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TransactionScript(SupportsInitializeInput = false, SupportsPreview = false)]
        public class ChartTx : ITransactionScript<Empty.Context, ISystemLogCriteria, ChartData>
        {
            #region Implementation of ITransactionScript<Context,ISystemLogCriteria,ChartData>

            public ISystemLogCriteria InitializeInput(Empty.Context context)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChartData Preview(ISystemLogCriteria input)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ChartData Execute(ISystemLogCriteria input)
            {
                return new ChartData() {
                    Labels = new List<string>() {
                        input.From.AddHours(1).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(2).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(3).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(4).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(5).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(6).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(7).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(8).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(9).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(10).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(11).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(12).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(13).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(14).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(15).ToString("yyyy-MM-dd HH:mm"),
                        input.From.AddHours(16).ToString("yyyy-MM-dd HH:mm")
                    },
                    //Series = new List<ChartData.t>() {
                    //    new ChartData.SeriesData() {
                    //        Type = ChartSeriesType.StackedBar,
                    //        Label = "Info",
                    //        Values = new List<decimal>() {
                    //            1000, 2000, 1500, 1000, 500, 1000, 1400, 1300, 1500, 1200, 900, 1000, 800, 500, 700, 1000
                    //        }
                    //    },
                    //    new ChartData.SeriesData() {
                    //        Type = ChartSeriesType.StackedBar,
                    //        Label = "Warning",
                    //        Values = new List<decimal>() {
                    //            200, 400, 300, 200, 100, 200, 280, 260, 300, 240, 180, 200, 160, 100, 140, 200
                    //        }
                    //    },
                    //    new ChartData.SeriesData() {
                    //        Type = ChartSeriesType.StackedBar,
                    //        Label = "Error",
                    //        Values = new List<decimal>() {
                    //            100, 200, 150, 100, 50, 100, 140, 130, 150, 120, 90, 100, 80, 50, 70, 100
                    //        }
                    //    }
                    //}
                };
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TransactionScript(SupportsInitializeInput = true, SupportsPreview = false)]
        public class MessageTx : ITransactionScript<Empty.Context, ISystemLogCriteria, IQueryable<ILogMessageEntity>>
        {
            private readonly IFramework _framework;
            private readonly IViewModelObjectFactory _viewModelFactory;

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public MessageTx(IFramework framework, IViewModelObjectFactory viewModelFactory)
            {
                _framework = framework;
                _viewModelFactory = viewModelFactory;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of ITransactionScript<Context,ISystemLogCriteria,ChartData>

            public ISystemLogCriteria InitializeInput(Empty.Context context)
            {
                var criteria = _viewModelFactory.NewEntity<ISystemLogCriteria>();
                criteria.Until = _framework.UtcNow;
                criteria.From = _framework.UtcNow.AddHours(-24);
                criteria.Node = _framework.CurrentNode.NodeName;
                return criteria;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<ILogMessageEntity> Preview(ISystemLogCriteria input)
            {
                throw new NotSupportedException();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable<ILogMessageEntity> Execute(ISystemLogCriteria input)
            {
                var messages = Enumerable.Range(0, 5).Select(x => _framework.NewDomainObject<ILogMessageEntity>()).ToArray();

                //messages[0].Timestamp = input.From.AddMilliseconds(1234);
                //messages[0].MessageId = "LoggerA.FirstMessage";

                //messages[1].Timestamp = input.From.AddMilliseconds(12345);
                //messages[1].MessageId = "LoggerA.SecondMessage";

                //messages[2].Timestamp = input.From.AddMilliseconds(123456);
                //messages[2].MessageId = "LoggerB.ThirdMessage";

                //messages[3].Timestamp = input.From.AddMilliseconds(1234567);
                //messages[3].MessageId = "LoggerC.FourthMessage";

                //messages[4].Timestamp = input.From.AddMilliseconds(12345678);
                //messages[4].MessageId = "LoggerD.FifthMessage";

                return messages.AsQueryable();
            }

            #endregion
        }
    }
}
