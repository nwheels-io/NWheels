using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface IAlertUiBehaviorBuilder<TModel, TState, TInput> : IBoundUiElementBuilder<TModel, TState>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> InfoInline(string text);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> InfoInline(
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> InfoDialog(string text);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> InfoDialog(
            string format, 
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> InfoDialog(AlertDialogButtons buttons, string text);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> InfoDialog(
            AlertDialogButtons buttons, 
            string format, 
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);

        IPromiseUiBehaviorBuilder<TModel, TState, TInput> ErrorInline(string text);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> ErrorInline(
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> ErrorDialog(string text);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> ErrorDialog(
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> ErrorDialog(AlertDialogButtons buttons, string text);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> ErrorDialog(
            AlertDialogButtons buttons,
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);

        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> QuestionDialog(AlertDialogButtons buttons, string text);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> QuestionDialog(
            AlertDialogButtons buttons,
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);

        IPromiseUiBehaviorBuilder<TModel, TState, TInput> WarningInline(string text);
        IPromiseUiBehaviorBuilder<TModel, TState, TInput> WarningInline(
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> WarningDialog(AlertDialogButtons buttons, string text);
        IPromiseUiBehaviorBuilder<TModel, TState, AlertDialogButtons> WarningDialog(
            AlertDialogButtons buttons,
            string format,
            params Expression<Func<IUiScope<TModel, TState, TInput>, object>>[] args);
    }
}
