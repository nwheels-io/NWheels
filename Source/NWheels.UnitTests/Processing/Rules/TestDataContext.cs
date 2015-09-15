using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UnitTests.Processing.Rules
{
    public class TestDataContext
    {
        public TestDataContext(int input)
        {
            Input = input;
            this.ActionLog = new List<string>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int Input { get; private set; }
        public List<string> ActionLog { get; private set; }
    }
}
