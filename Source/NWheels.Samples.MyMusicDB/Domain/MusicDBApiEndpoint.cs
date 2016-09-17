using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Stacks.AspNet;
using NWheels.UI;

namespace NWheels.Samples.MyMusicDB.Domain
{
    [SecurityCheck.AllowAnonymous]
    public class MusicDBApiEndpoint
    {
        public const string UserCookieName = "user";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IFramework _framework;
        private readonly IApiRequestLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MusicDBApiEndpoint(IFramework framework, IApiRequestLogger logger)
        {
            _framework = framework;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------\

        [HttpOperation(Route = "api/query/{entity}", Verbs = HttpOperationVerbs.Get)]
        public virtual HttpResponseMessage ApiQuery(HttpRequestMessage request)
        {
            var entityName = request
                .RequestUri
                .PathAndQuery
                .Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
                .Last();

            var queryParams = request.GetQueryString();
            var queryOptions = new ApplicationEntityService.QueryOptions(entityName, queryParams);

            var entityService = UidlApplicationComponent.Instance.EntityService;
            var responseJson = entityService.QueryEntityJson(entityName, queryOptions);

            var responseMessage = new HttpResponseMessage() {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.SetCookie(new Cookie(UserCookieName, "1"));

            RecordRequest(request);
            return responseMessage;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void RecordRequest(HttpRequestMessage request)
        {
            _logger.ApiRequestProcessed(request.RequestUri.PathAndQuery);

            var isNewUser = (request.GetCookie(UserCookieName) == null);

            if (isNewUser)
            {
                _logger.ApiUniqueUserDetected();
            }

            using (var context = _framework.NewUnitOfWork<IMusicDBContext>())
            {
                context.IncrementEventCounters(
                    deltaApiRequests: 1, 
                    deltaUniqueUsers: (isNewUser ? 1 : 0));
            }
        }
    }
}