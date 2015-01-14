using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Logging
{
    internal class LogNode
    {
        private readonly int _millisecondsTimestamp;
        private readonly ILazyLogText _lazyText;
        private readonly LogNodeType _nodeType;
        private LogLevel _level;
        private LogNode _nextSibling;
        private string _formattedBriefText = null;
        private string _formattedDetailsText = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode(int millisecondsTimestamp, LogNodeType nodeType, LogLevel level, ILazyLogText lazyText)
        {
            _millisecondsTimestamp = millisecondsTimestamp;
            _nodeType = nodeType;
            _level = level;
            _lazyText = lazyText;
            _nextSibling = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AttachNextSibling(LogNode sibling)
        {
            _nextSibling = sibling;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int MillisecondsTimestamp
        {
            get { return _millisecondsTimestamp; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BriefText
        {
            get
            {
                if ( _formattedBriefText == null )
                {
                    _formattedBriefText = _lazyText.FormatBrief();
                }
                
                return _formattedBriefText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string DetailsText
        {
            get
            {
                if ( _formattedDetailsText == null )
                {
                    _formattedDetailsText = _lazyText.FormatDetails();
                }

                return _formattedDetailsText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevel Level
        {
            get { return _level; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode NextSibling
        {
            get { return _nextSibling; }
        }
    }
}
