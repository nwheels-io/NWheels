using System;
using System.Reflection;

namespace NWheels.Processing.Jobs
{
    public abstract class ApplicationJobBase : IApplicationJob
    {
        protected ApplicationJobBase()
        {
            var attribute = this.GetType().GetCustomAttribute<ApplicationJobAttribute>();

            if (attribute != null)
            {
                this.JobId = attribute.IdName;
                this.Description = attribute.Description;
                this.IsReentrant = attribute.IsReentrant;
                this.NeedsPersistence = attribute.NeedsPersistence;
            }
            else
            {
                throw new InvalidOperationException("Cannot use empty constructor with a job that has no ApplicationJobAttribute applied.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
