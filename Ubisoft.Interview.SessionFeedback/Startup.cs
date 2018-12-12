using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Ubisoft.Interview.SessionFeedback.AppServices;
using Ubisoft.Interview.SessionFeedback.BL;
using Ubisoft.Interview.SessionFeedback.DA;
using Swashbuckle.AspNetCore.Filters;

namespace RestAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Ubisoft Feedback API",
                        Version = "v1",
                        Description =
                            "Test Project."
                    });

                c.ExampleFilters();

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Ubisoft.Interview.SessionFeedback.API.xml"));
            });
            services.AddSwaggerExamples();

            services.Configure<DatabaseConfig>(Configuration.GetSection("DatabaseConfig"));

            DAInstaller.Install(services);
            AppServiceInstaller.Install(services);
            BLInstaller.Install(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }


            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ubisoft  User´Feedback API");
            });

            app.UseHttpsRedirection();
            app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>();
                if (error != null)
                {
                    string errorMsg = "Unspecified error";
                    var statusCode = System.Net.HttpStatusCode.InternalServerError;

                    var exception = error.Error;
                    if (exception is ApiException apiException)
                    {
                        statusCode = apiException.StatusCode;
                        errorMsg = exception.Message;
                    }

                    context.Response.StatusCode = (int)statusCode;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync("{ \"Message\": \"" + errorMsg + "\" }");
                }
            });
        });

            app.UseMvc();
        }
    }
}
