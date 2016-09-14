using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class TransactionScreenPart<TContext, TInput, TScript> :
        ScreenPartBase<TransactionScreenPart<TContext, TInput, TScript>, TContext, Empty.Data, Empty.State>
        where TScript : ITransactionScript<TContext, TInput, Empty.Output>
        where TContext : class
        where TInput : class
    {
        public TransactionScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionScreenPart<TContext, TInput, TScript>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Transaction;

            presenter.On(base.NavigatedHere)
                .Broadcast(Transaction.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<TContext, TInput, TScript, Empty.Output> Transaction { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TransactionScreenPart<TContext, TInput, TScript, TOutput> :
        ScreenPartBase<TransactionScreenPart<TContext, TInput, TScript, TOutput>, TContext, Empty.Data, Empty.State>
        where TScript : ITransactionScript<TContext, TInput, TOutput>
        where TContext : class
        where TInput : class
        where TOutput : class
    {
        public TransactionScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionScreenPart<TContext, TInput, TScript, TOutput>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Transaction;

            presenter.On(base.NavigatedHere)
                .Broadcast(Transaction.ContextSetter).WithPayload(m => m.Input).TunnelDown();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<TContext, TInput, TScript, TOutput> Transaction { get; set; }
    }
}
