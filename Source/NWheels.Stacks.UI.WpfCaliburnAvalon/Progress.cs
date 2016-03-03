using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon
{
    public struct Progress
    {
        public Progress(int total, int completed)
        {
            Total = total;
            Completed = completed;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public readonly int Total;
        public readonly int Completed;
    }
}
