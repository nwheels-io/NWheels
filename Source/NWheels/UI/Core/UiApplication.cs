using System;
using NWheels.UI.Elements;

namespace NWheels.UI.Core
{
    public class UiApplication : IUiApplicationBuilder
    {
        #region IUiApplicationBuilder Members

        public void AddScreen(Action<IUiScreenBuilder<NWheels.UI.Unbound.Model, NWheels.UI.Unbound.State>> contents)
        {
            throw new NotImplementedException();
        }

        public void AddScreen(string screenId, Action<IUiScreenBuilder<NWheels.UI.Unbound.Model, NWheels.UI.Unbound.State>> contents)
        {
            throw new NotImplementedException();
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IUiElementBuilder Members

        public T CreateChildBuilder<T>(params object[] arguments) where T : IUiElementBuilder
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
