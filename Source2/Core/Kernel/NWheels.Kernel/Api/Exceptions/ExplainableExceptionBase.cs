using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace NWheels.Kernel.Api.Exceptions
{
    [Serializable]
    public abstract class ExplainableExceptionBase : Exception, IExplainableException
    {
        private readonly string _reason;
        private string _explanationPathAndQuery = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(string reason)
            : base(reason)
        {
            _reason = reason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(string reason, Exception innerException)
            : base(reason, innerException)
        {
            _reason = reason;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ExplainableExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExplanationPathAndQuery 
        {
            get
            {
                if (_explanationPathAndQuery == null)
                {
                    _explanationPathAndQuery = BuildExplanationPathAndQuery();
                }

                return _explanationPathAndQuery;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Reason => _reason;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract string[] BuildKeyValuePairs();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected string BuildExplanationPathAndQuery()
        {
            var minimalPath = $"{this.GetType().FullName}?Reason={HttpUtility.UrlEncode(this.Message)}";
            var keyValuePairs = BuildKeyValuePairs();

            if (keyValuePairs == null || keyValuePairs.Length == 0)
            {
                return minimalPath;
            }

            var builder = new StringBuilder(minimalPath);

            for (int i = 0 ; i < keyValuePairs.Length - 1 ; i++)
            {
                builder.Append($"&{keyValuePairs[i]}={HttpUtility.UrlEncode(keyValuePairs[i+1] ?? string.Empty)}");
            }

            return builder.ToString();
        }
    }
}