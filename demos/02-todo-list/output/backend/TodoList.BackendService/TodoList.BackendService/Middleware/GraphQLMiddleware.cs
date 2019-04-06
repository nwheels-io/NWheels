using System;
using System.IO;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TodoList.BackendService.Middleware
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;

        public GraphQLMiddleware(RequestDelegate next)
        {
            _next = next;
        }

		public async Task InvokeAsync(HttpContext httpContext, ISchema schema)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api/graphql") && string.Equals(httpContext.Request.Method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                using (var streamReader = new StreamReader(httpContext.Request.Body))
                {
                    var body = await streamReader.ReadToEndAsync();
                    var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

                    var result = await new DocumentExecuter().ExecuteAsync(doc => {
                        doc.Schema = schema;
                        doc.Query = request.Query;
                        doc.Inputs = request.Variables.ToInputs();
                    }).ConfigureAwait(false);

                    httpContext.Response.ContentType = "application/json";

                    var json = new DocumentWriter().Write(result);
                    await httpContext.Response.WriteAsync(json);
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    public class GraphQLRequest
    {
        public string Query;
        public JObject Variables;
    }
}
