using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Testing
{
    public class TestThreadLogAppender : IThreadLogAppender
    {
        private readonly List<LogNode> _logNodes = new List<LogNode>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendLogNode(LogNode node)
        {
            _logNodes.Add(node);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AppendActivityNode(ActivityLogNode activity)
        {
            _logNodes.Add(activity);
        }
    
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] GetLog()
        {
            return _logNodes.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode[] TakeLog()
        {
            var result = _logNodes.ToArray();
            _logNodes.Clear();
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] GetLogStrings()
        {
            return _logNodes.Select(node => node.SingleLineText).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] TakeLogStrings()
        {
            var result = _logNodes.Select(node => node.SingleLineText).ToArray();
            _logNodes.Clear();
            return result;
        }
    }
}
