using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public class ViewModel<TData, TState, TInput>
    {
        public TData Data { get; set; }
        public TState State { get; set; }
        public TInput Input { get; set; }
    }
}
