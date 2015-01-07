using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace NWheels.UI
{
    public interface IUiApplicationBuilder : IUiElementBuilder
    {
        void AddScreen(Action<IUiScreenBuilder<Unbound.Model, Unbound.State>> contents);
        void AddScreen(string screenId, Action<IUiScreenBuilder<Unbound.Model, Unbound.State>> contents);
        string Id { get; set; }
    }
}
