using NWheels.UI;
using NWheels.UI.Uidl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Toolbox;

namespace NWheels.Domains.DevOps.SystemLogs.UI
{
    public class SystemLogScreen : ScreenBase<SystemLogScreen, Empty.Input, Empty.Data, Empty.State>
    {
        public SystemLogScreen(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<SystemLogScreen, Empty.Data, Empty.State> presenter)
        {
            this.ScreenKind = ScreenKind.DashboardAdmin;

            ContentRoot = TabSet;
            
            TabSet.Tabs.Add(LogLevelSummary);
            TabSet.Tabs.Add(LogMessageSummary);
            TabSet.Tabs.Add(LogMessageList);
            TabSet.Tabs.Add(ThreadLogList);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TabbedScreenPartSet TabSet { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public LogLevelSummaryScreenPart LogLevelSummary { get; set; }
        public LogMessageSummaryScreenPart LogMessageSummary { get; set; }
        public LogMessageListScreenPart LogMessageList { get; set; }
        public ThreadLogListScreenPart ThreadLogList { get; set; }
    }
}
