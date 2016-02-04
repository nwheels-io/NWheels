using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;
using NWheels.Logging.Impl;

namespace NWheels.Logging
{
    public abstract class LogNode
    {
        public const char PreserveMessageIdPrefix = '!';

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _messageId;
        private LogContentTypes _contentTypes;
        private LogLevel _level;
        private LogOptions _options;
        private long _millisecondsTimestamp;
        private ulong _cpuCyclesTimestamp;
        private IThreadLog _threadLog = null;
        private LogNode _nextSibling = null;
        private ILogNameValuePair[] _listedNameValuePairs = null;
        private string _formattedSingleLineText = null;
        private string _formattedFullDetailsText = null;
        private string _formattedNameValuePairsText = null;
        private static string _s_commaSpaceSeparator = ", ";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected LogNode(string messageId, LogContentTypes contentTypes, LogLevel initialLevel, LogOptions options)
        {
            _messageId = messageId;
            _contentTypes = contentTypes;
            _level = initialLevel;
            _options = options;
            _millisecondsTimestamp = -1;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual ThreadLogSnapshot.LogNodeSnapshot TakeSnapshot()
        {
            var pairs = this.NameValuePairs;

            var snapshot = new ThreadLogSnapshot.LogNodeSnapshot {
                MessageId = _messageId,
                MillisecondsTimestamp = _millisecondsTimestamp,
                CpuCycles = (long)_cpuCyclesTimestamp,
                Level = _level,
                ContentTypes = _contentTypes,
                NameValuePairs = new List<ThreadLogSnapshot.NameValuePairSnapshot>(capacity: pairs.Length),
                ExceptionTypeName = (this.Exception != null ? this.Exception.GetType().FullName : null),
                ExceptionDetails = (this.Exception != null ? this.Exception.ToString() : null),
            };

            for ( int i = 0 ; i < pairs.Length ; i++ )
            {
                if ( !pairs[i].IsBaseValue() )
                {
                    snapshot.NameValuePairs.Add(new ThreadLogSnapshot.NameValuePairSnapshot {
                        Name = pairs[i].FormatName(),
                        Value = pairs[i].FormatValue(),
                        IsDetail = pairs[i].IsBaseValue() || !pairs[i].IsIncludedInSingleLineText()
                    });
                }
            }

            return snapshot;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DateTime GetUtcTimestamp()
        {
            return _threadLog.ThreadStartedAtUtc.AddMilliseconds(_millisecondsTimestamp);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return this.SingleLineText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MessageId
        {
            get
            {
                return _messageId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IThreadLog ThreadLog
        {
            get
            {
                return _threadLog;
            }
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

        public ulong CpuCyclesTimestamp
        {
            get
            {
                return _cpuCyclesTimestamp;
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

        public LogOptions Options
        {
            get
            {
                return _options;
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

        public ILogNameValuePair[] NameValuePairs
        {
            get
            {
                if ( _listedNameValuePairs == null )
                {
                    _listedNameValuePairs = ListNameValuePairs().ToArray();
                }

                return _listedNameValuePairs;
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

        public string NameValuePairsText
        {
            get
            {
                if ( _formattedNameValuePairsText == null )
                {
                    _formattedNameValuePairsText = FormatNameValuePairsText(delimiter: " ");
                }

                return _formattedNameValuePairsText;
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
            _threadLog = thread;
            _millisecondsTimestamp = thread.ElapsedThreadMilliseconds;
            _cpuCyclesTimestamp = thread.UsedThreadCpuCycles;
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

        protected string MessageIdToText()
        {
            if ( _messageId[0] != PreserveMessageIdPrefix )
            {
                return LogMessageHelper.GetTextFromMessageId(_messageId);
            }
            else
            {
                return _messageId.Substring(1);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string FormatSingleLineText()
        {
            var valuesInSingleLineText = string.Join(
                _s_commaSpaceSeparator, 
                this.NameValuePairs.Where(p => p.IsIncludedInSingleLineText()).Select(p => p.FormatLogString()));

            return MessageIdToText() + (string.IsNullOrEmpty(valuesInSingleLineText) ? string.Empty : ": " + valuesInSingleLineText);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string FormatFullDetailsText()
        {
            return FormatNameValuePairsText(delimiter: System.Environment.NewLine);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual string FormatNameValuePairsText(string delimiter)
        {
            return string.Join(delimiter, this.NameValuePairs.Select(p => p.FormatLogString()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IEnumerable<ILogNameValuePair> ListNameValuePairs()
        {
            var node = _threadLog.Node;

            var baseValues = new ILogNameValuePair[] {
                new LogNameValuePair<DateTime> {
                    Name = "$$time", 
                    Value = _threadLog.ThreadStartedAtUtc.AddMilliseconds(_millisecondsTimestamp), 
                    Format = "yyyy-MM-dd HH:mm:ss.fff"
                },
                new LogNameValuePair<string> {
                    Name = "$app", 
                    Value = node.ApplicationName
                },
                new LogNameValuePair<string> {
                    Name = "$node", 
                    Value = node.NodeName
                },
                new LogNameValuePair<string> {
                    Name = "$instance", 
                    Value = node.InstanceId
                },
                new LogNameValuePair<string> {
                    Name = "$env", 
                    Value = node.EnvironmentName
                },
                new LogNameValuePair<string> {
                    Name = "$message", 
                    Value = (_messageId[0] == PreserveMessageIdPrefix ? _messageId.Substring(1) : _messageId)
                },
                new LogNameValuePair<LogLevel> {
                    Name = "$level", 
                    Value = _level
                },
                new LogNameValuePair<Guid> {
                    Name = "$logid", 
                    Value = _threadLog.LogId,
                    Format = "N"
                },
            };

            return baseValues
                .ConcatIf(this.Exception != null, () => new LogNameValuePair<string> {
                    Name = "$exceptionType",
                    Value = this.Exception.GetType().FullName
                })
                .ConcatIf(this.Exception != null, () => new LogNameValuePair<string> {
                    Name = "$exception",
                    Value = this.Exception.Message
                });
        }

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
