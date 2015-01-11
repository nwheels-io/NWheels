using System;

namespace NWheels.UI.Elements
{
    public interface IUiApplicationBuilder : IUiElementBuilder
    {
        void AddScreen(Action<IUiScreenBuilder<Unbound.Model, Unbound.State>> contents);
        void AddScreen(string screenId, Action<IUiScreenBuilder<Unbound.Model, Unbound.State>> contents);
        string Id { get; set; }
    }
}
