namespace NWheels.UI.Elements
{
    public interface IUiElementBuilder
    {
        T CreateChildBuilder<T>(params object[] arguments) where T : IUiElementBuilder;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUiElementBuilder<TModel, TState, TFluent> : IUiElementBuilder
        where TFluent : IUiElementBuilder
    {
    }
}
