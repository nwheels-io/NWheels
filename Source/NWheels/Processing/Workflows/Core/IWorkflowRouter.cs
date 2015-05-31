using System;

namespace NWheels.Processing.Workflows.Core
{
    public interface IWorkflowRouter
    {
        void Route(IWorkflowRouterContext context);
        Type GetCookieType();
    }
}
