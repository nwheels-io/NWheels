using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Text;
using NWheels.UI.Elements;
using NWheels.UI.Layouts;

namespace NWheels.UI.Templates
{
    public interface IWizardUiScreenTemplate<TModel, TState> : IUiContainerBuilder<TModel, TState, IWizardUiScreenTemplate<TModel, TState>>
    {
        IWizardUiScreenTemplate<TModel, TState> SideBarTitle(string title);
        IWizardUiScreenTemplate<TModel, TState> DefaultNextButton(string caption);
        IWizardStepBuilder<TModel, TState> AddStep(string title);
        IUiLayoutBuilder<TModel, TState> Header();
        IWizardUiScreenTemplate<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IWizardUiScreenTemplate<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IWizardStepBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, IWizardStepBuilder<TModel, TState>>
    {
        IWizardStepBuilder<TModel, TState> Page(Action<IUiLayoutBuilder<TModel, TState>> contents);
        IWizardStepBuilder<TModel, TState> NextButton(string caption);
        IWizardStepBuilder<TModel, TState> NextButtonHidden();
    }
}
