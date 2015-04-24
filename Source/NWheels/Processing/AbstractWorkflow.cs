using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing
{
    public abstract class AbstractWorkflow : IWorkflowCodeBehind
    {

        public void OnBuildWorkflow(IWorkflowBuilder builder)
        {
            throw new NotImplementedException();
        }


        public Type SnapshotType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
