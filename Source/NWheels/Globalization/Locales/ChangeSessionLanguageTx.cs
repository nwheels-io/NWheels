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

namespace NWheels.Globalization.Locales
{
    [SecurityCheck.AllowAnonymous]
    public class ChangeSessionLanguageTx : TransactionScript<Empty.Context, ChangeSessionLanguageTx.IInput, Empty.Output>
    {
        private readonly ICoreSessionManager _sessionManager;
        private readonly ILocalizationProvider _localizationProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ChangeSessionLanguageTx(ICoreSessionManager sessionManager, ILocalizationProvider localizationProvider)
        {
            _sessionManager = sessionManager;
            _localizationProvider = localizationProvider;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,Output>

        public override Empty.Output Execute(IInput input)
        {
            var locale = _localizationProvider.GetLocale(input.IsoCode);
            
            _sessionManager.SetSessionUserInfo(Session.Current.Id, newCulture: locale.Culture);
            Thread.CurrentThread.CurrentCulture = locale.Culture;
            
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
