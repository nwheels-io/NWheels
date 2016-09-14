using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Exceptions;
using NWheels.Processing.Commands;

namespace NWheels.Stacks.AspNet
{
    public class UidlApplicationErrorHandler : ExceptionFilterAttribute
    {
        #region Overrides of ExceptionFilterAttribute

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is HttpResponseException)
            {
                return;
            }

            IFaultException fault = (actionExecutedContext.Exception as IFaultException);

            if (fault == null)
            {
                var isAuthenticated = Session.Current.UserPrincipal.Identity.IsAuthenticated;

                if (actionExecutedContext.Exception is SecurityException || !isAuthenticated)
                {
                    fault = new DomainFaultException<AuthorizationFault, AuthorizationFaultSubCode>(
                        faultCode: AuthorizationFault.AccessDenied,
                        faultSubCode: isAuthenticated ? AuthorizationFaultSubCode.None : AuthorizationFaultSubCode.NotAuthenticated);
                }
                else
                {
                    fault = new InternalServerErrorFaultException(actionExecutedContext.Exception);
                }
            }

            var result = new CommandResultMessage.Snapshot(
                faultType: fault.FaultType, 
                faultCode: fault.FaultCode, 
                faultSubCode: fault.FaultSubCode, 
                faultReason: fault.FaultReason);

            var json = JsonConvert.SerializeObject(result);

            actionExecutedContext.Response = new HttpResponseMessage() {
                StatusCode = (actionExecutedContext.Exception is SecurityException ? HttpStatusCode.Unauthorized : HttpStatusCode.InternalServerError),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            actionExecutedContext.Exception = null;
        }

        #endregion
    }
}
