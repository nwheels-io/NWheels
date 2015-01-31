using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public abstract class ApplicationJobBase : IApplicationJob
    {
        protected ApplicationJobBase(string jobId, string description, bool isReentrant = false, bool needsPersistence = false)
        {
            this.JobId = jobId;
            this.Description = description;
            this.IsReentrant = isReentrant;
            this.NeedsPersistence = needsPersistence;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void Execute(IApplicationJobContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string JobId { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Description { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsReentrant { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool NeedsPersistence { get; protected set; }
    }
}
