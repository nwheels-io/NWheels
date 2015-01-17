using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    public abstract class LogNode
    {
        private LogContentTypes _contentTypes;
        private LogLevel _level;
        private long _millisecondsTimestamp;
        private LogNode _nextSibling = null;
        private string _formattedSingleLineText = null;
        private string _formattedFullDetailsText = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected LogNode(LogContentTypes contentTypes, LogLevel initialLevel)
        {
            _contentTypes = contentTypes;
            _level = initialLevel;
            _millisecondsTimestamp = -1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ThreadLogSnapshot.LogNodeSnapshot TakeSnapshot()
        {
            return new ThreadLogSnapshot.LogNodeSnapshot {
                MillisecondsTimestamp = _millisecondsTimestamp,
                Level = _level,
                ContentTypes = _contentTypes,
                SingleLineText = this.SingleLineText,
                FullDetailsText = this.FullDetailsText,
                ExceptionTypeName = (this.Exception != null ? this.Exception.GetType().FullName : null)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long MillisecondsTimestamp
        {
            get
            {
                return _millisecondsTimestamp;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogLevel Level
        {
            get
            {
                return _level;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogContentTypes ContentTypes
        {
            get
            {
                return _contentTypes;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string SingleLineText
        {
            get
            {
                if ( _formattedSingleLineText == null )
                {
                    _formattedSingleLineText = FormatSingleLineText();
                }

                return _formattedSingleLineText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FullDetailsText
        {
            get
            {
                if ( _formattedFullDetailsText == null )
                {
                    _formattedFullDetailsText = FormatFullDetailsText();
                }

                return _formattedFullDetailsText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExceptionTypeName
        {
            get
            {
                return (this.Exception != null ? this.Exception.GetType().FullName : null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LogNode NextSibling
        {
            get
            {
                return _nextSibling;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Exception Exception
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ThreadTaskType TaskType
        {
            get
            {
                return ThreadTaskType.Unspecified;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal virtual void AttachToThreadLog(IThreadLog thread, ActivityLogNode parent)
        {
            _millisecondsTimestamp = thread.ElapsedThreadMilliseconds;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void AttachNextSibling(LogNode sibling)
        {
            if ( _nextSibling == null )
            {
                _nextSibling = sibling;
            }
            else
            {
                throw new InvalidOperationException("This log node already has attached sibling node");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract string FormatSingleLineText();
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract string FormatFullDetailsText();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleLogLevelFrom(LogLevel subNodeLevel)
        {
            if ( subNodeLevel > _level )
            {
                _level = subNodeLevel;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BubbleContentTypesFrom(LogContentTypes subNodeContentTypes)
        {
            _contentTypes |= subNodeContentTypes;
        }
    }
}
