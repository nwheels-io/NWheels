using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Samples.MyMusicDB.Domain
{
    public interface IApiRequestLogger : IApplicationEventLogger
    {
        [LogInfo(Aggregate = true)]
        void ApiRequestProcessed(string pathAndQuery);

        [LogInfo(Aggregate = true)]
        void ApiUniqueUserDetected();
    }
}
