using System.Runtime.Serialization;
using NWheels.Domains.DevOps.SystemLogs.Entities;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI
{
    public class SystemLogScreenPart : ScreenPartBase<SystemLogScreenPart, Empty.Context, Empty.Data, Empty.State>
    {
        public SystemLogScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ScreenPartBase<SystemLogScreenPart,Input,Data,State>

        protected override void DescribePresenter(PresenterBuilder<SystemLogScreenPart, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Tabs;

            Tabs.Add(LogLevelSummary);
            Tabs.Add(LogMessageSummary);
            Tabs.Add(LogMessageList);

            presenter.On(base.NavigatedHere)
                .Navigate().FromContainer(Tabs.Container).ToScreenPart<Empty.Context>(LogLevelSummary);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public TabbedScreenPartSet Tabs { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public LogLevelSummaryScreenPart LogLevelSummary { get; set; }
        [DataMember]
        public LogMessageSummaryScreenPart LogMessageSummary { get; set; }
        [DataMember]
        public LogMessageListScreenPart LogMessageList { get; set; }
    }
}
