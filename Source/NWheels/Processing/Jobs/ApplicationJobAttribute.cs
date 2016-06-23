using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Jobs
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationJobAttribute : Attribute
    {
        public ApplicationJobAttribute(string idName)
        {
            if (string.IsNullOrWhiteSpace(idName))
            {
                throw new ArgumentException("Job ID cannot be null or empty.", "idName");
            }

            this.IdName = idName;
            this.Description = idName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string IdName { get; private set; }
        public string Description { get; set; }
        public bool IsReentrant { get; set; }
        public bool NeedsPersistence { get; set; }
    }
}
