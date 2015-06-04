using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface ICodeBehind<in TElement>
        where TElement : IUIElement
    {
        void OnDescribeUI(TElement element);
    }
}
