using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Api;
using Google.Cloud.Diagnostics.Common;
using Google.Cloud.Diagnostics.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.CodeAnalysis;
using Google.Cloud.SecretManager.V1;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Presentation
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment host)
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", 
                host.ContentRootPath+@"/plasma-apex-340711-e75ba0e2c66c.json");


            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });


            string projectId = Configuration["project"];

            services.AddGoogleErrorReportingForAspNetCore(
                new Google.Cloud.Diagnostics.Common.ErrorReportingServiceOptions
            {
                // Replace ProjectId with your Google Cloud Project ID.
                ProjectId = projectId,
                // Replace Service with a name or identifier for the service.
                ServiceName = "EJ-Assignment",
                // Replace Version with a version for the service.
                Version = "1"
            });


            services.AddLogging(builder => builder.AddGoogle(new LoggingServiceOptions
            {
                // Replace ProjectId with your Google Cloud Project ID.
                ProjectId = projectId,
                // Replace Service with a name or identifier for the service.
                ServiceName = "EJ-Assignment",
                // Replace Version with a version for the service.
                Version = "1"
            }));


            services.AddControllersWithViews();


            // Create the client.
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            // Build the resource name.
            SecretVersionName secretVersionName = new SecretVersionName(projectId, "MyKeys", "1");

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();

            dynamic myObj = JsonConvert.DeserializeObject(payload);
            string clientId = Convert.ToString(myObj["Authentication:Google:ClientId"]);
            string clientSecret = Convert.ToString(myObj["Authentication:Google:ClientSecret"]);
            //string redis = Convert.ToString(myObj["redis"]);


            // requires
            // using Microsoft.AspNetCore.Authentication.Cookies;
            // using Microsoft.AspNetCore.Authentication.Google;
            // NuGet package Microsoft.AspNetCore.Authentication.Google
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                });

            services.AddRazorPages();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env){


            app.UseDeveloperExceptionPage();
            app.UseHsts();
            /*if (env.IsDevelopment())
            {
            app.UseDeveloperExceptionPage();
                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }*/
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // TODO: Use your User Agent library of choice here.
                
                    // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                    options.SameSite = SameSiteMode.Unspecified;
                
            }
        }
    }
}
