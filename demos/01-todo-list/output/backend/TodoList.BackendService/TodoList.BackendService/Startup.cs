using System;
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
using TodoList.BackendService.Middleware;
using TodoList.BackendService.Repositories;
using TodoList.BackendService.Schemas;

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

            //services.AddSingleton<ITodoItemRepository>(new MockInMemoryRepository());
            
            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<ISchema, TodoListSchema>();
            services.AddScoped<TodoListQuery>();
            services.AddScoped<TodoListMutation>();
            services.AddScoped<TodoItemGraph>();
            services.AddScoped<TodoItemInputGraph>();
            services.AddScoped<OrderByDirectionType>();
            services.AddScoped<TodoItemOrderGraph>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CorsPolicyName); 
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMiddleware<GraphQLMiddleware>();            
        }
    }
}
