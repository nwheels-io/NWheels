using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Uidl.Generic
{
    public class TransactionWizard<TViewModel> : AbstractUIElement<TViewModel>
    {
        public PromiseBuilder OnSubmitCallTx<TTx>(Expression<Func<TTx, Task>> call)
        {
            return new PromiseBuilder();
        }

        public PromiseBuilder OnSubmit(params Expression<Func<PromiseBuilder>>[] codeBlock)
        {
            return new PromiseBuilder();
        }

        [NestedElement]
        public CommandElement SubmitCommand { get; set; }
    }
}
