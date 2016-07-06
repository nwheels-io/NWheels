using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NWheels.DataObjects;
using NWheels.Endpoints;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Globalization.Core;
using NWheels.Logging;
using NWheels.Processing;
using NWheels.Stacks.Apis.MicrosoftTranslator.MicrosoftTranslatorService;
using NWheels.UI;
using NWheels.UI.Factories;

namespace NWheels.Stacks.Apis.MicrosoftTranslator.Domain
{
    [TransactionScript(SupportsInitializeInput = true, SupportsPreview = false, AuditName = "LocaleAutoTranslate")]
    public class LocaleBulkTranslateTx : TransactionScript<Empty.Context, LocaleBulkTranslateTx.IInput, LocaleBulkTranslateTx.IOutput>
    {
        private readonly IFramework _framework;
        private readonly IViewModelObjectFactory _viewModelFactory;
        private readonly IHttpBotLogger _httpLogger;
        private readonly ICoreLocalizationProvider _localizationProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public LocaleBulkTranslateTx(
            IFramework framework, 
            IViewModelObjectFactory viewModelFactory, 
            IHttpBotLogger httpLogger, 
            ICoreLocalizationProvider localizationProvider)
        {
            _framework = framework;
            _viewModelFactory = viewModelFactory;
            _httpLogger = httpLogger;
            _localizationProvider = localizationProvider;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TransactionScript<Context,IInput,IOutput>

        public override IInput InitializeInput(Empty.Context context)
        {
            var input = _viewModelFactory.NewEntity<IInput>();
            return input;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IOutput Execute(IInput input)
        {
            var output = _viewModelFactory.NewEntity<IOutput>();

            try
            {
                string accessToken;

                if (!GetAccessToken(input, output, out accessToken))
                {
                    return output;
                }

                if (CallTranslationService(accessToken, input, output))
                {
                    StoreTranslatedLocale(input.TranslateTo);
                }
            }
            catch (Exception e)
            {
                output.Status = SeverityLevel.Error;
                output.Details = string.Format("Translation failed. Error '{0}': {1}", e.GetType().Name, e.Message);
            }

            return output;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool GetAccessToken(IInput input, IOutput output, out string accessToken)
        {
            var bot = new HttpBot(_httpLogger);
            bot.BaseUrl = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            
            var formData = new {
                grant_type = "client_credentials",
                client_id = input.MicrosoftTranslatorServiceAuthentication.ClientId,
                client_secret = input.MicrosoftTranslatorServiceAuthentication.ClientSecret,
                scope = "http://api.microsofttranslator.com"
            };

            try
            { 
                dynamic response = bot.Post(relativeUrl: "", query: null, form: formData).ResponseBodyAsJsonDynamic();
                accessToken = response.access_token;
                return true;
            }
            catch (Exception e)
            {
                output.Status = SeverityLevel.Error;
                output.Details = string.Format("Authorization failed with error '{0}': {1}", e.GetType().Name, e.Message);
                
                accessToken = null;
                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool CallTranslationService(string accessToken, IInput input, IOutput output)
        {
            LanguageServiceClient client = new LanguageServiceClient(
                new BasicHttpBinding() {
                    ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                    MaxReceivedMessageSize = Int32.MaxValue
                },
                new EndpointAddress("http://api.microsofttranslator.com/V2/soap.svc"));

            HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
            httpRequestProperty.Method = "POST";
            httpRequestProperty.Headers.Add("Authorization", "Bearer\x20" + accessToken);

            string[] entryIds = input.TranslateFrom.Entries.Keys.ToArray();
            string[] sourceTexts = input.TranslateFrom.Entries.Values.ToArray();
            List<string> translatedTexts = new List<string>();

            try
            {
                using (new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;

                    foreach (var sourceTextsChunk in sourceTexts.TakeChunks(maxChunkTextLength: 10000))
                    {
                        var translatedChunk = PerformTranslationChunkRequest(input, client, sourceTextsChunk);
                        translatedTexts.AddRange(translatedChunk.Select(t => t.TranslatedText));
                    }
                }

                for (int i = 0; i < translatedTexts.Count; i++)
                {
                    input.TranslateTo.Entries[entryIds[i]] = translatedTexts[i];
                }

                output.Status = SeverityLevel.Success;
                output.Details = string.Format("Translation completed successfully. {0} entries translated.", entryIds.Length);

                return true;
            }
            catch (Exception e)
            {
                output.Status = SeverityLevel.Error;
                output.Details = string.Format("Translation failed. Microsoft Translation Service returned error. {0}: {1}.", e.GetType().Name, e.Message);

                return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TranslateArrayResponse[] PerformTranslationChunkRequest(IInput input, LanguageServiceClient client, string[] sourceTexts)
        {
            TranslateOptions translateArrayOptions = new TranslateOptions(); // Use the default options
            TranslateArrayResponse[] translatedTexts = client.TranslateArray(
                string.Empty, // AppID, deprecated 
                sourceTexts,
                input.TranslateFrom.CultureCode,
                input.TranslateTo.CultureCode,
                translateArrayOptions);
            
            return translatedTexts;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StoreTranslatedLocale(IApplicationLocaleEntity locale)
        {
            using (var context = _framework.NewUnitOfWork<IApplicationLocalizationContext>())
            {
                context.Locales.Update(locale);
                context.CommitChanges();
            }

            _localizationProvider.Refresh();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IInput
        {
            [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
            IApplicationLocaleEntity TranslateFrom { get; set; }

            [PropertyContract.Required, PropertyContract.Relation.AggregationParent]
            IApplicationLocaleEntity TranslateTo { get; set; }
            
            IMicrosoftTranslatorAuthenticationEntityPart MicrosoftTranslatorServiceAuthentication { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityPartContract]
        public interface IMicrosoftTranslatorAuthenticationEntityPart
        {
            [PropertyContract.Required]
            string ClientId { get; set; }

            [PropertyContract.Required]
            string ClientSecret { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IOutput
        {
            [PropertyContract.ReadOnly]
            SeverityLevel Status { get; set; }

            [PropertyContract.ReadOnly]
            string Details { get; set; }
        }

    }
}
