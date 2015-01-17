using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Core.Logging
{
    internal class UniversalThreadLogAnchor : IThreadLogAnchor
    {
        public ThreadLog CurrentThreadLog
        {
            get
            {
                System.Web.HttpContext httpContext;
                System.ServiceModel.OperationContext operationContext;

                if ( (httpContext = System.Web.HttpContext.Current) != null )
                {
                    return (ThreadLog)httpContext.Items[s_HttpContextKey];
                }
                else if ( (operationContext = System.ServiceModel.OperationContext.Current) != null )
                {
                    var extension = operationContext.Extensions.Find<ThreadLogAnchorExtension>();
                    return (extension != null ? extension.CurrentThreadLog : null);
                }
                else
                {
                    return s_CurrentThreadLog;
                }
            }
            set
            {
                System.Web.HttpContext httpContext;
                System.ServiceModel.OperationContext operationContext;

                if ( (httpContext = System.Web.HttpContext.Current) != null )
                {
                    httpContext.Items[s_HttpContextKey] = value;
                }
                else if ( (operationContext = System.ServiceModel.OperationContext.Current) != null )
                {
                    var extension = operationContext.Extensions.Find<ThreadLogAnchorExtension>();

                    if ( extension == null )
                    {
                        extension = new ThreadLogAnchorExtension();
                        operationContext.Extensions.Add(extension);
                    }

                    extension.CurrentThreadLog = value;
                }
                else
                {
                    s_CurrentThreadLog = value;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static ThreadLog s_CurrentThreadLog;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly object s_HttpContextKey = new object();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ThreadLogAnchorExtension : System.ServiceModel.IExtension<System.ServiceModel.OperationContext>
        {
            void System.ServiceModel.IExtension<System.ServiceModel.OperationContext>.Attach(System.ServiceModel.OperationContext owner)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void System.ServiceModel.IExtension<System.ServiceModel.OperationContext>.Detach(System.ServiceModel.OperationContext owner)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLog CurrentThreadLog { get; set; }
        }
    }
}
