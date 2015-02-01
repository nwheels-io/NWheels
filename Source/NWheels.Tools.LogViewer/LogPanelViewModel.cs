using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NWheels.Logging;

namespace NWheels.Tools.LogViewer
{
    public class LogPanelViewModel
    {
        private readonly List<ThreadLogModelTuple> _threadLogModelTuples;

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public LogPanelViewModel()
        {
            _threadLogModelTuples = new List<ThreadLogModelTuple>();
            Items = new ObservableCollection<TreeNodeItem<ThreadLogViewModel.NodeItem>>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddThreadLog(ThreadLogSnapshot threadLog)
        {
            var tuple = new ThreadLogModelTuple(this, threadLog);
            _threadLogModelTuples.Add(tuple);
            Items.Add(tuple.RootNodeItem);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public ObservableCollection<TreeNodeItem<ThreadLogViewModel.NodeItem>> Items { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadLogModelTuple
        {
            public ThreadLogModelTuple(LogPanelViewModel ownerModel, ThreadLogSnapshot threadLog)
            {
                this.ThreadLog = threadLog;
                this.ViewModel = new ThreadLogViewModel(threadLog);

                var lastTuple = ownerModel._threadLogModelTuples.LastOrDefault();

                this.RootNodeItem = new TreeNodeItem<ThreadLogViewModel.NodeItem>(
                    ViewModel.RootActivity, 
                    ownerModel.Items, 
                    parentNode: null, 
                    prevSiblingNode: (lastTuple != null ? lastTuple.RootNodeItem : null));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLogSnapshot ThreadLog { get; private set; }
            public ThreadLogViewModel ViewModel { get; private set; }
            public TreeNodeItem<ThreadLogViewModel.NodeItem> RootNodeItem { get; private set; }
        }
    }
}
