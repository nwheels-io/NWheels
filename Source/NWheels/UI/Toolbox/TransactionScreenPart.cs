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
        ScreenPartBase<TransactionScreenPart<TContext, TInput, TScript>, TContext, Empty.Input, Empty.State>
        where TScript : ITransactionScript<TContext, TInput, Empty.Output>
        where TContext : class
        where TInput : class
    {
        private bool _useFlatStyle;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionScreenPart<TContext, TInput, TScript> UseFlatStyle()
        {
            Transaction.TemplateName = "TransactionFormFlatStyle";
            Transaction.Execute.Icon = "check";
            Transaction.Reset.Icon = "trash";

            _useFlatStyle = true;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionScreenPart<TContext, TInput, TScript>, Empty.Input, Empty.State> presenter)
        {
            ContentRoot = Transaction;

            presenter.On(base.NavigatedHere)
                .Broadcast(Transaction.ContextSetter).WithPayload(m => m.Input).TunnelDown();

            if (_useFlatStyle)
            {
                presenter.Defer(() => {
                    Action<IUidlForm> configureInputForm = (form) => {
                        form.Commands.Remove(Transaction.Execute);
                        form.Commands.Remove(Transaction.Reset);
                    };

                    if (Transaction.InputForm != null)
                    {
                        configureInputForm(Transaction.InputForm);
                    }
                    if (Transaction.InputFormTypeSelector != null)
                    {
                        Transaction.InputFormTypeSelector.ForEachWidgetOfType<IUidlForm>(configureInputForm);
                    }
                });
            }
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
        private bool _useFlatStyle;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionScreenPart<TContext, TInput, TScript, TOutput> UseFlatStyle()
        {
            Transaction.TemplateName = "TransactionFormFlatStyle";
            Transaction.Execute.Icon = "check";
            Transaction.Reset.Icon = "trash";

            _useFlatStyle = true;

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<TransactionScreenPart<TContext, TInput, TScript, TOutput>, Empty.Data, Empty.State> presenter)
        {
            ContentRoot = Transaction;

            presenter.On(base.NavigatedHere)
                .Broadcast(Transaction.ContextSetter).WithPayload(m => m.Input).TunnelDown();

            Transaction.TemplateName = "TransactionFormFlatStyle";
            Transaction.Execute.Icon = "check";
            Transaction.Reset.Icon = "trash";

            if (_useFlatStyle)
            {
                presenter.Defer(() => {
                    Action<IUidlForm> configureInputForm = (form) => {
                        form.Commands.Remove(Transaction.Execute);
                        form.Commands.Remove(Transaction.Reset);
                    };

                    if (Transaction.InputForm != null)
                    {
                        configureInputForm(Transaction.InputForm);
                    }
                    if (Transaction.InputFormTypeSelector != null)
                    {
                        Transaction.InputFormTypeSelector.ForEachWidgetOfType<IUidlForm>(configureInputForm);
                    }
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TransactionForm<TContext, TInput, TScript, TOutput> Transaction { get; set; }
    }
}
