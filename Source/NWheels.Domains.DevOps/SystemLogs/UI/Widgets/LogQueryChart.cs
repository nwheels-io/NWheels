#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.DevOps.SystemLogs.Transactions;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.SystemLogs.UI.Widgets
{
    public class LogQueryChart : Chart
    {
        public LogQueryChart(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<Chart, Empty.Data, Empty.State> presenter)
        {
            base.DescribePresenter(presenter);
            presenter.On(Loaded)
                .InvokeTransactionScript<AbstractLogMessageSummaryTx>()
                .WaitForReply((tx, vm) => tx.Execute(null))
                .Then(b => b.Broadcast(DataReceived).WithPayload(vm => vm.Input).TunnelDown());
        }
    }
}

#endif