﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TodoList.BackendService.Graphs;
using TodoList.BackendService.Queries;
using TodoList.BackendService.Repositories;

namespace TodoList.BackendService
{
    public class Startup
    {
        private const string CorsPolicyName = "RestApiCorsPolicy";
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy(
                CorsPolicyName, 
                policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:3000")
            ));            
            
            services.AddSingleton<ITodoItemRepository>(new TodoItemRepository(new MongoDBConfig {
                Host = "localhost",
                Database = "todo_list",
                Credential = new MongoDBConfig.CredentialConfig {
                    Database = "admin",
                    UserName = "root",
                    Password = "example"
                }
            }));
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseMvc();
            //var schema = new Schema { Query = new HelloWorldQuery() };

            app.UseCors(CorsPolicyName); 
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run(async (context) => {
                if (context.Request.Path.StartsWithSegments("/api/graphql")
                    && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    string body;
                    using (var streamReader = new StreamReader(context.Request.Body))
                    {
                        body = await streamReader.ReadToEndAsync();

                        var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);
                        var schema = new Schema { Query = new TodoItemQuery() };

                        var result = await new DocumentExecuter().ExecuteAsync(doc => {
                            doc.Schema = schema;
                            doc.Query = request.Query;
                        }).ConfigureAwait(false);

                        var json = new DocumentWriter(indent: true).Write(result);
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(json);
                    }
                }
            });
        }
    }
}
