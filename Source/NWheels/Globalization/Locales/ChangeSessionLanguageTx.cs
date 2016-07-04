using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Authorization.Core;
using NWheels.Core;
using NWheels.Processing;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Globalization.Locales
{
    [SecurityCheck.AllowAnonymous]
    [TransactionScript(SupportsInitializeInput = true)]
    public class ChangeSessionLanguageTx : TransactionScript<Empty.Context, ChangeSessionLanguageTx.IInput, Empty.Output>
    {
        private readonly ICoreSessionManager _sessionManager;
        private readonly ILocalizationProvider _localizationProvider;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChangeSessionLanguageTx(ICoreSessionManager sessionManager, ILocalizationProvider localizationProvider, IViewModelObjectFactory viewModelFactory)
        {
            _sessionManager = sessionManager;
            _localizationProvider = localizationProvider;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,Output>

        public override IInput InitializeInput(Empty.Context context)
        {
            return _viewModelFactory.NewEntity<IInput>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Empty.Output Execute(IInput input)
        {
            var locale = _localizationProvider.GetLocale(input.IsoCode);

            _sessionManager.SetSessionUserInfo(Session.Current.Id, newCulture: locale.Culture);
            Thread.CurrentThread.CurrentUICulture = locale.Culture;
            
            return null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            string IsoCode { get; set; }
        }
    }
}
