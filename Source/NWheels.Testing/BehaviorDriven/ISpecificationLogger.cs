using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Testing.BehaviorDriven
{
    public interface ISpecificationLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.Unspecified)]
        ILogActivity TestingSpecification(string name);

        [LogInfo]
        void SpecificationPassed(string name);

        [LogError]
        void SpecificationFailed(string name, Exception exception);


        [LogInfo]
        void Running(string step);

        [LogInfo]
        void Passed(string step);

        [LogError]
        void Failed(string step, Exception exception);

        [LogVerbose]
        void Reusing(string step);
    }
}
